# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`DevInstance.BlazorToolkit` is a NuGet-published class library (`Microsoft.NET.Sdk.Razor`, `net10.0`) that provides infrastructure for Blazor apps: attribute-based service registration, a service-execution/state-management pattern for components, a fluent HTTP API-context layer, form validation helpers, and an offline-first stack (IndexedDB store, connectivity, read-through cache, write-through outbox). It targets both Blazor Server and WASM from the same package (`browser` is a supported platform).

The package version lives in `src/DevInstance.BlazorToolkit.csproj` (`<Version>`) and `GeneratePackageOnBuild` is on, so a Release build produces a `.nupkg`.

## Build, test, run

```bash
# Build the whole solution
dotnet build DevInstance.BlazorToolkit.sln

# Build the library only in Release (produces the NuGet package)
dotnet build src/DevInstance.BlazorToolkit.csproj -c Release

# Run all tests
dotnet test tests/DevInstance.BlazorToolkit.Offline.Tests/DevInstance.BlazorToolkit.Offline.Tests.csproj

# Run a single test by name (xUnit v3)
dotnet test tests/DevInstance.BlazorToolkit.Offline.Tests/DevInstance.BlazorToolkit.Offline.Tests.csproj --filter "FullyQualifiedName~OutboxProcessor"

# Run the sample app (hosted WASM) — Server project hosts the Client
dotnet run --project example/DevInstance.BlazorToolkit.Samples/DevInstance.BlazorToolkit.Samples.csproj
```

Tests use **xUnit v3**. The only test project covers offline outbox dispatch behavior (success / retry+backoff / conflict / drop) against an in-memory object store — no browser needed. CI (`azure-pipeline-ci.yml`) restores + builds the solution in Release with `.NET 10.x`; it does not run tests.

There is no separate lint step; rely on the compiler with `Nullable` enabled. Note the codebase itself is not fully null-annotated (many public APIs use `= null` defaults on non-nullable reference params) — match the existing style rather than introducing new nullable warnings.

## Architecture

Consuming apps depend on three interlocking pieces. Read `docs/USAGE.md` for the full API surface with examples; the big picture:

### 1. Attribute-based DI (`Tools/`)
Services are marked `[BlazorService]` (optionally `[BlazorService(ServiceLifetime.Singleton|Transient)]`, default **Scoped**). `AddBlazorServices()` in `Program.cs` scans **the calling assembly** via reflection and registers each marked class against **every interface it implements** (or the class itself if it has none). `AddBlazorServicesMocks()` does the same for `[BlazorServiceMock]` — the swap point for test/mock registrations. Because scanning uses `Assembly.GetCallingAssembly()`, services are discovered from the app assembly that calls `AddBlazorServices`, not from this library.

### 2. Service execution + component state (`Services/`, `Components/`)
This is the core pattern. A component implements `IServiceExecutionHost` (usually by inheriting `ServiceExecutionHostComponent`), which exposes `InProgress`, `IsError`, `ErrorMessage`, `ServiceState`, `ComponentState`/`State` (prerender persistence), and `ShowLogin()`.

- `ServiceCall.cs` holds the extension methods components call: `ServiceReadAsync` / `ServiceSubmitAsync` (and `BeginServiceCall(...).DispatchCall(...).ExecuteAsync()` for manual chaining).
- `ServiceExecutionHandler` orchestrates the calls. It wraps each call in a `StateGuard` (sets `InProgress`/`ServiceState`, calls `StateHasChanged`, resets on dispose), restores/persists results via `stateKey` for prerendering, catches exceptions into a failed `ServiceActionResult`, and on failure routes: `!IsAuthorized` → `ShowLogin()`; otherwise the optional `error` callback can suppress the default error banner by returning `true`.
- **Chained `DispatchCall`s run strictly sequentially and stop on the first failure** — later calls can safely read results set by earlier ones. `CallContext<T>` bundles all call parameters into a reusable object (useful for a "refresh the list" action reused after create/update/delete).
- Everything returns `ServiceActionResult<T>` (`Result`, `Success`, `Errors[]`, `IsAuthorized`) with factory helpers `OK`, `Failed`, `Unauthorized`. This envelope is the contract between services, components, the HTTP layer, and the offline cache.

### 3. Service body helpers — Server vs WASM split (`Services/Server/`, `Services/Wasm/`)
A service method's body wraps its real work in a `ServiceUtils` helper that normalizes exceptions/logging into a `ServiceActionResult`. **Pick by host:**
- `Services.Server.ServiceUtils.HandleServiceCallAsync` — server-side services calling repositories/DB directly.
- `Services.Wasm.ServiceUtils.HandleWebApiCallAsync` — WASM services calling a REST API via `IApiContext<T>`.

### HTTP layer (`Http/`)
`IApiContext<T>` is a fluent request builder: `Api.Get()/Post()/Put()/Delete()` → `.Path()/.Parameter()/.Query()/.Top()/.Page()/.Search()/.Sort()` (extension methods in `Http/Extensions/`) → `.ExecuteAsync()` / `.ExecuteListAsync()`. Contexts are produced by `HttpApiContextFactory` (constructed with a named `HttpClient` + base path). `ApiUrlBuilder` is the standalone fluent URL composer. `Query(object)` serializes a query model to params, honoring `[QueryName("...")]` and comma-joining arrays.

### Validation (`Validators/`)
`ServiceResultValidationEx` maps `ServiceActionError[]` (from a failed submit) onto an `EditForm`. Bootstrap-flavored helpers: `BootstrapFieldCssClassProvider`, `BoostrapValidationMessage` (note the spelling in the API).

### Offline stack (`Offline/`) — WASM only
Registered separately via `AddBlazorOffline(opts => ...)` + per-entity `AddCacheableSource<T>()` / `AddCrudSyncHandler<T>()`. Layers: `IObjectStore`/`IndexedDbObjectStore` (JSON key/value over IndexedDB), `IConnectivityService` (`navigator.onLine`), `ICacheableSource<T>` (read-through: serve local now, refresh in background), `IOutboxQueue`/`OutboxProcessor` (write-through with exponential backoff; `pending`/`failed`/`conflict`), `ISyncOperationHandler`/`CrudSyncHandler<T>` (per-entity replay; CRUD handler is last-write-wins), `IMasterDataSync` (refresh all sources on login/when stale).

- Requires the JS assets loaded **before** `blazor.webassembly.js`:
  `_content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js` and `...-connectivity.js`. **TypeScript sources are in `src/Scripts/`; the compiled JS lives in `src/wwwroot/js/`** — edit the `.ts`, not the shipped `.js`.
- After `builder.Build()` you must `InitializeAsync()` the `IObjectStore` and `IConnectivityService`.
- Known pitfall (documented in `docs/offline-tools.md`): a background refresh can clobber a locally-created record before it syncs — a source's `SaveLocal` should merge and preserve items with a still-pending outbox entry.
- Server responses must be wrapped in `ServiceActionResult<ModelList<T>>` / `ServiceActionResult<T>`; `CacheableSource` unwraps that envelope.

## Key external dependencies
- `DevInstance.WebServiceToolkit.Common` — `ModelList<T>`, common model/query types used across services.
- `DevInstance.LogScope` — the `IScopeLog` tracing (`log.TraceScope()`) threaded through the execution handler and `ServiceUtils`.
- `FluentValidation`, `Microsoft.AspNetCore.Components.WebAssembly`, `Microsoft.Extensions.Http`/`DependencyInjection.Abstractions`.

## Conventions
- Namespaces follow folders: `DevInstance.BlazorToolkit.{Services,Services.Server,Services.Wasm,Http,Http.Extensions,Tools,Validators,Offline.Storage,Offline.Connectivity,Offline.Sync,Offline.Extensions,Components,Utils,Exceptions}`.
- Some public identifiers carry historical typos that are part of the shipped API — do not "fix" them without a version bump: `sucessAsync` parameter, `BoostrapValidationMessage`.
- The `example/` projects (`Samples` server host + `Samples.Client` WASM) are the reference for wiring everything end-to-end.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`DevInstance.BlazorToolkit` is a NuGet-published class library (`Microsoft.NET.Sdk.Razor`, `net10.0`) that provides infrastructure for Blazor apps: attribute-based service registration, a service-execution/state-management pattern for components, a fluent HTTP API-context layer, form validation helpers, and an offline-first stack (IndexedDB store, connectivity, read-through cache, write-through outbox). It targets both Blazor Server and WASM from the same package (`browser` is a supported platform).

A second package, `DevInstance.BlazorToolkit.Components` (`src/Components/`, split out in 10.6.0), holds the UI components (`ErrorMessageBanner`, `PageProgress`, `LoadingProgress`, `DataPager`, `ModelDataPager`, `SortableHeader`, `BlazorToolkitPageLayout`). It `ProjectReference`s core and packs with a NuGet dependency on it. `ServiceExecutionHostComponent` deliberately **stays in core** (it is the base of the service-execution pattern); the `DevInstance.BlazorToolkit.Components` namespace is therefore split across both assemblies. Since 10.7.0 it also holds `HDataGrid` and friends (`DataGrid/`, namespace `….Components.DataGrid`, ported from ThreadIQ) and `HSearchBar`/`SearchHighlight` (`Search/`, `….Components.Search`); history and decisions in `docs/components-package-plan.md`.
- Grid assets: `wwwroot/css/blazortoolkit-components.css` (consumer links it; colours via `--btk-*` with Bootstrap fallbacks) and the ES module `wwwroot/js/blazortoolkit-components.js`, which `HDataGrid` imports itself (no `<script>` tag). The module has a TS source at `src/Components/Scripts/blazortoolkit-components.ts`; keep the two in sync by hand. Consumers need Bootstrap 5.3 CSS **and** JS bundle plus bootstrap-icons.
- Search highlights come from `IModelListHighlights` / `HighlightRange` in WST.Common (≥ 10.4.0); the grid checks `Data is IModelListHighlights`.
- Pure grid logic lives in the internal `DataGrid/GridLayout.cs`, tested by `tests/DevInstance.BlazorToolkit.Components.Tests` via `InternalsVisibleTo`. Sample page: `Samples.Client/Pages/Grid.razor` (`/grid`).

Both packages share one version number: bump `<Version>`/`<AssemblyVersion>`/`<FileVersion>` in **both** `src/Core/DevInstance.BlazorToolkit.csproj` and `src/Components/DevInstance.BlazorToolkit.Components.csproj`. `GeneratePackageOnBuild` is on, so a Release build produces both `.nupkg`s.

## Build, test, run

```bash
# Build the whole solution
dotnet build DevInstance.BlazorToolkit.sln

# Build the libraries in Release (produces both NuGet packages)
dotnet build src/Components/DevInstance.BlazorToolkit.Components.csproj -c Release

# Run all tests
dotnet test tests/DevInstance.BlazorToolkit.Offline.Tests/DevInstance.BlazorToolkit.Offline.Tests.csproj

# Run a single test by name (xUnit v3)
dotnet test tests/DevInstance.BlazorToolkit.Offline.Tests/DevInstance.BlazorToolkit.Offline.Tests.csproj --filter "FullyQualifiedName~OutboxProcessor"

# Run the sample app (hosted WASM) — Server project hosts the Client
dotnet run --project example/DevInstance.BlazorToolkit.Samples/DevInstance.BlazorToolkit.Samples.csproj
```

Note: since the xunit.v3 4.x upgrade, `dotnet test` fails on the .NET 10 SDK ("Testing with VSTest target is no longer supported by Microsoft.Testing.Platform") until a `global.json` opts into the new `dotnet test`; meanwhile run a test project as an executable: `dotnet run --project tests/<project>`.

Tests use **xUnit v3**. The single test project (named `Offline.Tests`) covers offline outbox dispatch behavior (success / retry+backoff / conflict / drop) against an in-memory object store, plus `HttpApiContext` list deserialization and error-body handling via a stub `HttpMessageHandler` — no browser needed. CI (`azure-pipeline-ci.yml`) restores + builds the solution in Release with `.NET 10.x`; it does not run tests. Publishing is a manual Azure pipeline (`azure-pipeline-release.yml`) that takes the package version as a parameter.

There is no separate lint step; rely on the compiler with `Nullable` enabled. Note the codebase itself is not fully null-annotated (many public APIs use `= null` defaults on non-nullable reference params) — match the existing style rather than introducing new nullable warnings.

## Architecture

Folder paths below are relative to the core project, `src/Core/`.

Consuming apps depend on three interlocking pieces. Read `docs/USAGE.md` for the full API surface with examples; the big picture:

### 1. Attribute-based DI (`Tools/`)
Services are marked `[BlazorService]` (optionally `[BlazorService(ServiceLifetime.Singleton|Transient)]`, default **Scoped**). `AddBlazorServices()` in `Program.cs` scans **the calling assembly** via reflection and registers each marked class against **every interface it implements** (or the class itself if it has none). `AddBlazorServicesMocks()` does the same for `[BlazorServiceMock]` — the swap point for test/mock registrations. Because scanning uses `Assembly.GetCallingAssembly()`, services are discovered from the app assembly that calls `AddBlazorServices`, not from this library.

### 2. Service execution + component state (`Services/`, `Components/ServiceExecutionHostComponent.razor`)
This is the core pattern. A component implements `IServiceExecutionHost` (usually by inheriting `ServiceExecutionHostComponent`), which exposes `InProgress`, `IsError`, `ErrorMessage`, `ServiceState`, `ComponentState`/`State` (prerender persistence), and `ShowLogin()`.

- `ServiceCall.cs` holds the extension methods components call: `ServiceReadAsync` / `ServiceSubmitAsync` (and `BeginServiceCall(...).DispatchCall(...).ExecuteAsync()` for manual chaining).
- `ServiceExecutionHandler` orchestrates the calls. It wraps each call in a `StateGuard` (sets `InProgress`/`ServiceState`, calls `StateHasChanged`, resets on dispose), restores/persists results via `stateKey` for prerendering, catches exceptions into a failed `ServiceActionResult` (as a `ServiceActionError` of type `ServiceActionErrorType.Exception`, with the original exception on `.Exception`), and on failure routes: `!IsAuthorized` → `ShowLogin()`; otherwise the optional `error` callback can suppress the default error banner by returning `true`.
- **Chained `DispatchCall`s run strictly sequentially and, by default, stop on the first failure** — later calls can safely read results set by earlier ones. `ExecuteAsync(ignoreFailures: true)` runs every call regardless and returns `false` if any failed. `CallContext<T>` bundles all call parameters into a reusable object (useful for a "refresh the list" action reused after create/update/delete).
- Everything returns `ServiceActionResult<T>` (`Result`, `Success`, `Errors[]`, `IsAuthorized`) with factory helpers `OK`, `Failed`, `Unauthorized`. This envelope is the contract between services, components, the HTTP layer, and the offline cache.

### 3. Service body helpers — Server vs WASM split (`Services/Server/`, `Services/Wasm/`)
A service method's body wraps its real work in a `ServiceUtils` helper that normalizes exceptions/logging into a `ServiceActionResult`. **Pick by host:**
- `Services.Server.ServiceUtils.HandleServiceCallAsync` — server-side services calling repositories/DB directly.
- `Services.Wasm.ServiceUtils.HandleWebApiCallAsync` — WASM services calling a REST API via `IApiContext<T>`.

### HTTP layer (`Http/`)
`IApiContext<T>` is a fluent request builder: `Api.Get()/Post()/Put()/Delete()` → `.Path()/.Parameter()/.Query()/.Top()/.Page()/.Search()/.Sort()` (extension methods in `Http/Extensions/`) → `.ExecuteAsync()` / `.ExecuteAsync<TList>()` for list responses, or `.ExecuteModelListAsync()` which returns `IModelList<T>` backed by the internal `ApiModelList<T>` (a default interface method, so it can't be restored from prerender `stateKey` state — that deserializes by declared type). `ExecuteListAsync()` is `[Obsolete]` because it returns the obsolete `ModelList<T>`. Contexts are produced by `HttpApiContextFactory` (constructed with a named `HttpClient` + base path). `ApiUrlBuilder` is the standalone fluent URL composer. `Query(object)` serializes a query model to params, honoring `[QueryName("...")]` and comma-joining arrays.

### Validation (`Validators/`)
`ServiceResultValidationEx` maps `ServiceActionError[]` (from a failed submit) onto an `EditForm`. Bootstrap-flavored helpers: `BootstrapFieldCssClassProvider`, `BoostrapValidationMessage` (note the spelling in the API).

### Offline stack (`Offline/`) — WASM only
Registered separately via `AddBlazorOffline(opts => ...)` + per-entity `AddCacheableSource<T>()` / `AddCrudSyncHandler<T>()`. Layers: `IObjectStore`/`IndexedDbObjectStore` (JSON key/value over IndexedDB), `IConnectivityService` (`navigator.onLine`), `ICacheableSource<T>` (read-through: serve local now, refresh in background), `IOutboxQueue`/`OutboxProcessor` (write-through with exponential backoff; `pending`/`failed`/`conflict`), `ISyncOperationHandler`/`CrudSyncHandler<T>` (per-entity replay; CRUD handler is last-write-wins), `IMasterDataSync` (refresh all sources on login/when stale).

- Requires the JS assets loaded **before** `blazor.webassembly.js`:
  `_content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js` and `...-connectivity.js`. The shipped JS lives in `src/Core/wwwroot/js/`. `blazortoolkit-db.js` has a TypeScript source at `src/Core/Scripts/blazortoolkit-db.ts`; keep the two in sync (there is no tsconfig/npm build step in the repo, so the `.js` is not regenerated automatically). `blazortoolkit-connectivity.js` has no TS source — edit it directly.
- After `builder.Build()` you must `InitializeAsync()` the `IObjectStore` and `IConnectivityService`.
- Known pitfall (documented in `docs/offline-tools.md`): a background refresh can clobber a locally-created record before it syncs — a source's `SaveLocal` should merge and preserve items with a still-pending outbox entry.
- `CacheableSource` reads a **bare** `IModelList<T>` from its list endpoint into the internal `CacheableList<T>` (only `Items` is read), so it depends on neither `ModelList<T>` nor a `ServiceActionResult` envelope (required before 10.5.1). A body without `items` fails the refresh rather than being read as an empty list — otherwise a mismatched server would wipe the cache.

## Key external dependencies
- `DevInstance.WebServiceToolkit.Common` — `IModelItem` / `IModelList<T>` (the concrete `ModelItem` / `ModelList<T>` are `[Obsolete]` as of 10.3.0; apps define their own list classes), common model/query types. Its companion server package `DevInstance.WebServiceToolkit` returns a `WebServiceError` body on every error whose `ErrorType` values are numerically identical to `ServiceActionErrorType` — do not renumber that enum.
- `DevInstance.LogScope` — the `IScopeLog` tracing (`log.TraceScope()`) threaded through the execution handler and `ServiceUtils`.
- `FluentValidation`, `Microsoft.AspNetCore.Components.WebAssembly`, `Microsoft.Extensions.Http`/`DependencyInjection.Abstractions`.

## Conventions
- Namespaces follow folders: `DevInstance.BlazorToolkit.{Services,Services.Server,Services.Wasm,Http,Http.Extensions,Tools,Validators,Offline.Storage,Offline.Connectivity,Offline.Sync,Offline.Extensions,Components,Utils,Exceptions}`.
- Some public identifiers carry historical typos that are part of the shipped API — do not "fix" them without a version bump: `sucessAsync` parameter, `BoostrapValidationMessage`.
- Moving a UI component across assemblies does not break the consumer's build: Razor only warns `RZ10012` and renders the tag as plain HTML. Build the sample with `-p:WarningsAsErrors=RZ10012` to check.
- The `example/` projects (`Samples` server host + `Samples.Client` WASM) are the reference for wiring everything end-to-end.

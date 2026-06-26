# BlazorToolkit Usage Guide

A comprehensive guide to using the BlazorToolkit library for service execution, state management, and HTTP communication in Blazor applications.

## Table of Contents

- [Installation & Setup](#installation--setup)
- [Service Execution](#service-execution)
- [HTTP API Context](#http-api-context)
- [State Management](#state-management)
- [Offline Tools](#offline-tools)
- [Error Handling](#error-handling)
- [Complete Examples](#complete-examples)

---

## Installation & Setup

### 1. Register Services

BlazorToolkit uses attribute-based service discovery. Mark your services with `[BlazorService]` and register them in `Program.cs`:

```csharp
// Program.cs (Server)
builder.Services.AddHttpClient("BlazorToolkitClient");
builder.Services.AddBlazorServices();
```

```csharp
// Program.cs (Client/WASM)
builder.Services.AddHttpClient("MyAppClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped<HttpApiContextFactory>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new HttpApiContextFactory(factory, "MyAppClient", "api");
});

builder.Services.AddBlazorServices();
```

### 2. Create a Service

Services are marked with the `[BlazorService]` attribute and automatically registered during startup. By default, services are registered with **Scoped** lifetime.

```csharp
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Services;

// Default: Scoped lifetime
[BlazorService]
public class TodoService : ITodoService
{
    // Service implementation
}
```

#### Service Lifetimes

You can specify different service lifetimes using the attribute constructor or property:

```csharp
// Singleton - single instance for the entire application
[BlazorService(ServiceLifetime.Singleton)]
public class CacheService : ICacheService
{
    // Shared state across all requests
}

// Transient - new instance for each injection
[BlazorService(ServiceLifetime.Transient)]
public class TemporaryService : ITemporaryService
{
    // Fresh instance every time
}

// Scoped - instance per request/scope (default)
[BlazorService(ServiceLifetime.Scoped)]
public class UserContextService : IUserContextService
{
    // Shared within a request
}

// Alternative property syntax
[BlazorService(Lifetime = ServiceLifetime.Singleton)]
public class ConfigService : IConfigService
{
    // Configuration service
}
```

**Choosing the Right Lifetime:**
- **Singleton**: Use for stateless services, caches, or configuration that should be shared globally
- **Scoped**: Use for services that maintain state during a user session or request (default for most Blazor services)
- **Transient**: Use for lightweight, stateless services where you need a fresh instance each time

---

## Service Execution

The service execution pattern provides automatic progress tracking, error handling, and state management for async operations.

### Core Concepts

- **`IServiceExecutionHost`** - Interface implemented by components to handle service call state
- **`ServiceExecutionHandler`** - Orchestrates service calls with automatic state management
- **`ServiceActionResult<T>`** - Unified result type for all service operations

### Using ServiceReadAsync

Use `ServiceReadAsync` for data fetching operations:

```csharp
@inherits ServiceExecutionHostComponent
@inject ITodoService TodoService

@code {
    private ModelList<TodoItem>? todos;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        await this.ServiceReadAsync(
            handler: async () => await TodoService.GetItemsAsync(),
            success: result => todos = result
        );
    }
}
```

### Using ServiceSubmitAsync

Use `ServiceSubmitAsync` for create/update/delete operations:

```csharp
private TodoItem newTodo = new();

private async Task AddTodo()
{
    await this.ServiceSubmitAsync(
        handler: async () => await TodoService.AddAsync(newTodo),
        success: result =>
        {
            todos = result;
            newTodo = new TodoItem(); // Reset form
        }
    );
}
```

### Full Method Signatures

```csharp
// Read operation
await this.ServiceReadAsync<T>(
    handler: async () => await service.GetAsync(),  // Required: the async operation
    success: result => { },                          // Optional: success callback
    stateKey: "myKey",                               // Optional: key for state persistence
    sucessAsync: async result => { },                // Optional: async success callback
    error: errors => { return false; },              // Optional: error handler (return true to suppress)
    before: () => { },                               // Optional: runs before the call
    enableProgress: true,                            // Optional: show progress indicator
    log: scopeLog                                    // Optional: logging
);

// Submit operation (same signature)
await this.ServiceSubmitAsync<T>(...);
```

### Using CallContext

For cleaner code with many parameters, use `CallContext<T>`:

```csharp
await this.ServiceReadAsync(new CallContext<ModelList<TodoItem>>
{
    Handler = async () => await TodoService.GetItemsAsync(),
    Success = result => todos = result,
    StateKey = nameof(todos),
    EnableProgress = true,
    Error = errors =>
    {
        // Custom error handling
        return false;
    }
});
```

#### Advanced CallContext Usage: Reusable Actions

`CallContext` is particularly powerful when you need to reuse the same action across multiple sequential operations. A common pattern is refreshing a list after modifying it:

```csharp
// Define a reusable refresh action
private CallContext<ModelList<TodoItem>> refreshTodos = new()
{
    Handler = async () => await TodoService.GetItemsAsync(currentQuery),
    Success = result => todos = result,
    StateKey = nameof(todos)
};

// Delete an item and refresh the list
private async Task DeleteTodo(string id)
{
    await this.BeginServiceCall(ServiceExecutionType.Submitting)
        .DispatchCall(
            handler: async () => await TodoService.DeleteAsync(id),
            success: _ => { /* Delete succeeded */ }
        )
        .DispatchCall(refreshTodos)  // Reuse the refresh action
        .ExecuteAsync();
}

// Update an item and refresh the list
private async Task UpdateTodo(TodoItem item)
{
    await this.BeginServiceCall(ServiceExecutionType.Submitting)
        .DispatchCall(
            handler: async () => await TodoService.UpdateAsync(item),
            success: _ => { /* Update succeeded */ }
        )
        .DispatchCall(refreshTodos)  // Reuse the same refresh action
        .ExecuteAsync();
}
```

**Key Benefits:**
- **DRY Principle**: Define the refresh logic once, reuse it everywhere
- **Consistency**: All operations use the same refresh mechanism
- **Maintainability**: Change the refresh logic in one place

### Manual Service Execution

For advanced scenarios, use `BeginServiceCall` directly:

```csharp
await this.BeginServiceCall(ServiceExecutionType.Reading)
    .DispatchCall(
        handler: async () => await Service1.GetAsync(),
        success: r => result1 = r
    )
    .DispatchCall(
        handler: async () => await Service2.GetAsync(),
        success: r => result2 = r
    )
    .ExecuteAsync();
```

#### Sequential Execution Guarantee

**Important**: All `DispatchCall` operations execute **sequentially** in the order they are chained. This means:

1. Each call completes before the next one starts
2. You can safely use results from previous calls
3. If any call fails, subsequent calls are **not executed**

```csharp
private TodoItem? selectedItem;

// Example: Load item details, then load related comments
await this.BeginServiceCall(ServiceExecutionType.Reading)
    .DispatchCall(
        handler: async () => await TodoService.GetByIdAsync(itemId),
        success: result => selectedItem = result  // Store the result
    )
    .DispatchCall(
        handler: async () => await CommentService.GetCommentsAsync(selectedItem.Id),
        success: result => comments = result  // Safely use selectedItem from previous call
    )
    .ExecuteAsync();
```

This sequential behavior makes it safe to chain dependent operations without worrying about race conditions or synchronization.

---

## HTTP API Context

The HTTP layer provides a fluent API for building and executing HTTP requests.

### Creating an API Context

#### Via Factory (Recommended)

```csharp
// In Program.cs
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<HttpApiContextFactory>();
    return factory.Create<TodoItem>("MyAppClient", "api/todos");
});

// Or with default configuration
builder.Services.AddScoped<HttpApiContextFactory>(sp =>
{
    var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new HttpApiContextFactory(httpFactory, "MyAppClient", "api");
});

// Then create contexts with relative URLs
var todosApi = factory.CreateDefault<TodoItem>("todos");
var usersApi = factory.CreateDefault<User>("users");
```

### Basic CRUD Operations

```csharp
[BlazorService]
public class TodoService : ITodoService
{
    private readonly IApiContext<TodoItem> Api;

    public TodoService(IApiContext<TodoItem> api)
    {
        Api = api;
    }

    // GET all items
    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetAllAsync()
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await Api.Get().ExecuteListAsync()
        );
    }

    // GET single item by ID
    public async Task<ServiceActionResult<TodoItem?>> GetByIdAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await Api.Get(id).ExecuteAsync()
        );
    }

    // POST new item
    public async Task<ServiceActionResult<TodoItem?>> CreateAsync(TodoItem item)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await Api.Post(item).ExecuteAsync()
        );
    }

    // PUT update item
    public async Task<ServiceActionResult<TodoItem?>> UpdateAsync(TodoItem item)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await Api.Put(item, item.Id).ExecuteAsync()
        );
    }

    // DELETE item
    public async Task<ServiceActionResult<bool>> DeleteAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) =>
            {
                await Api.Delete(id).ExecuteAsync();
                return true;
            }
        );
    }
}
```

### URL Building

#### Using ApiUrlBuilder

```csharp
var url = ApiUrlBuilder.Create("api/companies")
    .Path(companyId)
    .Path("employees")
    .Query("department", "Engineering")
    .Query("active", true)
    .ToString();

// Result: api/companies/123/employees?department=Engineering&active=True
```

#### Fluent URL Building with IApiContext

```csharp
// Build complex URLs
var result = await Api
    .Get()
    .Path("active")
    .Parameter("category", "work")
    .Parameter("priority", "high")
    .ExecuteListAsync();
```

### Query Parameters

#### Manual Parameters

```csharp
var result = await Api
    .Get()
    .Parameter("top", 10)
    .Parameter("page", 0)
    .Parameter("search", "meeting")
    .ExecuteListAsync();
```

#### Using Extension Methods

```csharp
using DevInstance.BlazorToolkit.Http.Extensions;

var result = await Api
    .Get()
    .Top(10)
    .Page(0)
    .Search("meeting")
    .Sort("createdAt", ascending: false)
    .ExecuteListAsync();
```

#### Using Query Objects

Define a query model:

```csharp
public class TodoQueryModel
{
    public int Top { get; set; }
    public int Page { get; set; }
    public string? Search { get; set; }

    [QueryName("filter")]  // Custom parameter name
    public string? Category { get; set; }

    public string[]? Include { get; set; }  // Arrays become comma-separated
}
```

Use with the `Query()` extension:

```csharp
var query = new TodoQueryModel
{
    Top = 10,
    Page = 0,
    Search = "meeting",
    Include = new[] { "subtasks", "comments" }
};

var result = await Api.Get().Query(query).ExecuteListAsync();
// URL: api/todos?top=10&page=0&search=meeting&include=subtasks,comments
```

---

## State Management

### Prerendering State Persistence

BlazorToolkit supports state persistence across prerendering using the `stateKey` parameter:

```csharp
// The state key identifies data to persist
await this.ServiceReadAsync(
    handler: async () => await TodoService.GetItemsAsync(),
    success: result => todos = result,
    stateKey: nameof(todos)  // Data is cached and restored after prerendering
);
```

### Progress and State Tracking

Components inheriting from `ServiceExecutionHostComponent` have automatic state tracking:

```razor
@inherits ServiceExecutionHostComponent

@if (InProgress)
{
    <LoadingSpinner />
}
else if (IsError)
{
    <ErrorMessage Text="@ErrorMessage" />
}
else
{
    <TodoList Items="@todos" />
}

<!-- Or check specific operation types -->
@if (ServiceState == ServiceExecutionType.Reading)
{
    <span>Loading...</span>
}
else if (ServiceState == ServiceExecutionType.Submitting)
{
    <span>Saving...</span>
}
```

### Disabling Progress Indication

For background operations that shouldn't show loading states:

```csharp
await this.ServiceReadAsync(
    handler: async () => await Service.RefreshAsync(),
    success: result => data = result,
    enableProgress: false  // Won't set InProgress = true
);
```

---

## Offline Tools

The `DevInstance.BlazorToolkit.Offline` namespace provides offline-first building blocks for Blazor WebAssembly apps: an IndexedDB object store, connectivity tracking, a read-through cache, and a write-through outbox (sync queue). Reads serve local data immediately and refresh from the API in the background; writes save locally and queue for replay when connectivity returns.

### Architecture

| Layer | Type | Role |
|-------|------|------|
| Storage | `IObjectStore` / `IndexedDbObjectStore` | Generic JSON key/value store over IndexedDB |
| Storage | `OfflineStoreOptions` | Declares DB name/version and object stores (plus reserved `syncMeta`, `syncQueue`) |
| Connectivity | `IConnectivityService` | `navigator.onLine` + online/offline events → `OnConnectivityChanged` |
| Sync | `ICacheableSource<T>` / `CacheableSource<T>` | Read-through cache: serve local immediately, refresh in background |
| Sync | `IOutboxQueue` / `OutboxProcessor` | Write-through queue with exponential backoff; `pending`/`failed`/`conflict` states |
| Sync | `ISyncOperationHandler` / `CrudSyncHandler<T>` | Per-entity replay logic; `CrudSyncHandler<T>` covers standard REST CRUD |
| Sync | `IMasterDataSync` / `MasterDataSync` | Refreshes all registered cacheable sources (on login / when stale) |

The server is expected to wrap responses in `ServiceActionResult<ModelList<T>>` / `ServiceActionResult<T>` — `CacheableSource` unwraps that envelope.

### 1. Register the Offline Stack

```csharp
// Program.cs (WASM)
builder.Services.AddBlazorOffline(opts =>
{
    opts.DatabaseName = "myapp";
    opts.DatabaseVersion = 1;
    opts.AddStore("todos");
    opts.AddStore("contacts");
});

// Read-through cache per entity
builder.Services.AddCacheableSource<TodoItem>(sp =>
{
    var store = sp.GetRequiredService<IObjectStore>();
    return new CacheableSourceOptions<TodoItem>
    {
        Endpoint = "todos",
        LoadLocal = () => store.GetAllAsync<TodoItem>("todos"),
        SaveLocal = items => store.ReplaceAllAsync("todos", items),
    };
});

// Outbox handler per entity (standard CRUD)
builder.Services.AddCrudSyncHandler<TodoItem>(sp =>
{
    var store = sp.GetRequiredService<IObjectStore>();
    return new CrudSyncHandlerOptions<TodoItem>
    {
        EntityType = "todo",
        Endpoint = "todos",
        LoadLocal = id => store.GetAsync<TodoItem>("todos", id),
    };
});
```

Initialize once after `builder.Build()`:

```csharp
var host = builder.Build();
await host.Services.GetRequiredService<IObjectStore>().InitializeAsync();
await host.Services.GetRequiredService<IConnectivityService>().InitializeAsync();
await host.RunAsync();
```

### 2. Reference the Static Assets

The IndexedDB and connectivity JS ship as static web assets. Load them **before** `blazor.webassembly.js` so the first interop call during host init succeeds:

```html
<script src="_content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js"></script>
<script src="_content/DevInstance.BlazorToolkit/js/blazortoolkit-connectivity.js"></script>
```

TypeScript sources live in `src/Scripts/`.

### 3. Feature Service Pattern

Inject `ICacheableSource<T>`, `IObjectStore`, and `IOutboxQueue`. Reads return the cache immediately and fire a background refresh; writes save locally and enqueue for sync:

```csharp
[BlazorService]
public class TodoService : ITodoService
{
    private readonly ICacheableSource<TodoItem> _source;
    private readonly IObjectStore _store;
    private readonly IOutboxQueue _outbox;

    public TodoService(ICacheableSource<TodoItem> source, IObjectStore store, IOutboxQueue outbox)
    {
        _source = source;
        _store = store;
        _outbox = outbox;
    }

    // Read: returns cached items immediately, refreshes in the background
    public async Task<List<TodoItem>> GetItemsAsync()
    {
        var items = await _source.GetCachedAsync();
        _ = _source.RefreshAsync(); // fire-and-forget background refresh
        return items;
    }

    // Write: persist locally, then queue for server sync
    public async Task SaveAsync(TodoItem item, bool isNew)
    {
        await _store.PutAsync("todos", item);
        await _outbox.EnqueueAsync("todo", item.Id, isNew ? "create" : "update");
    }

    public async Task DeleteAsync(string id)
    {
        await _store.DeleteAsync("todos", id);
        await _outbox.EnqueueAsync("todo", id, "delete");
    }
}
```

> **Avoid clobbering offline-created records:** a background refresh can wipe a locally-created record before it syncs. Make the source's `SaveLocal` merge — keep local items that still have a pending outbox entry and aren't on the server yet.

### 4. Surface Sync State in the UI

`IOutboxQueue`, `IConnectivityService`, and `IMasterDataSync` all expose state and events for sync chips and banners:

```razor
@inject IOutboxQueue Outbox
@inject IConnectivityService Connectivity
@implements IDisposable

@if (!Connectivity.IsOnline)
{
    <span class="badge bg-warning">Offline</span>
}
@if (Outbox.PendingCount > 0)
{
    <span class="badge bg-info">@Outbox.PendingCount pending</span>
}
@if (Outbox.ConflictCount > 0)
{
    <span class="badge bg-danger">@Outbox.ConflictCount conflicts</span>
}

@code {
    protected override void OnInitialized()
    {
        Outbox.OnStatusChanged += StateHasChanged;
        Outbox.OnProcessed += StateHasChanged;
        Connectivity.OnConnectivityChanged += _ => StateHasChanged();
    }

    public void Dispose()
    {
        Outbox.OnStatusChanged -= StateHasChanged;
        Outbox.OnProcessed -= StateHasChanged;
    }
}
```

### 5. Master-Data Sync

Pull all registered cacheable sources into the local cache — typically on login and periodically:

```csharp
// Force a full refresh (e.g. after login)
await MasterDataSync.SyncAllAsync();

// Refresh only if the last sync is older than 5 minutes
await MasterDataSync.SyncIfStaleAsync(TimeSpan.FromMinutes(5));
```

### 6. Conflicts

`OutboxProcessor` parks an entry as `conflict` when a handler returns `SyncOperationResult.Conflict(...)` (e.g. server state moved). `CrudSyncHandler<T>` is last-write-wins and never produces conflicts; richer custom handlers can. Surface `GetConflictsAsync()` in the UI and resolve with `DiscardAsync(entryId)`:

```csharp
var conflicts = await Outbox.GetConflictsAsync();
foreach (var entry in conflicts)
{
    // Show the conflict to the user, then discard or re-apply
    await Outbox.DiscardAsync(entry.Id);
}
```

See [docs/offline-tools.md](offline-tools.md) for the full reference, and `tests/DevInstance.BlazorToolkit.Offline.Tests` for outbox dispatch behavior (success / retry+backoff / conflict / drop) tested with an in-memory store.

---

## Error Handling

### ServiceActionResult

All service operations return `ServiceActionResult<T>`:

```csharp
public class ServiceActionResult<T>
{
    public T? Result { get; set; }
    public bool Success { get; set; }
    public ServiceActionError[]? Errors { get; set; }
    public bool IsAuthorized { get; set; }
}
```

### Creating Results in Services

```csharp
// Success
return ServiceActionResult<TodoItem>.OK(newTodo);

// Failure with message
return ServiceActionResult<TodoItem>.Failed("Item not found");

// Failure with validation error
return ServiceActionResult<TodoItem>.Failed(new ServiceActionError
{
    ErrorType = ServiceActionErrorType.Validation,
    PropertyName = nameof(TodoItem.Title),
    Message = "Title is required"
});

// Unauthorized
return ServiceActionResult<TodoItem>.Unauthorized();
```

### Custom Error Handling in Components

```csharp
await this.ServiceSubmitAsync(
    handler: async () => await TodoService.AddAsync(newTodo),
    success: result => todos = result,
    error: errors =>
    {
        // Handle validation errors
        foreach (var error in errors)
        {
            if (error.ErrorType == ServiceActionErrorType.Validation)
            {
                validationMessages[error.PropertyName] = error.Message;
            }
        }

        // Return true to suppress the default error display
        // Return false to show the error in ErrorMessage
        return true;
    }
);
```

### Integration with Form Validation

Use `ServiceResultValidation` to display errors in forms:

```razor
<EditForm Model="@newTodo" OnValidSubmit="@AddTodo">
    <ServiceResultValidationEx @ref="validator" CssProvider="new BootstrapFieldCssClassProvider()" />

    <InputText @bind-Value="newTodo.Title" class="form-control" />
    <BoostrapValidationMessage For="@(() => newTodo.Title)" />

    <button type="submit" disabled="@InProgress">Add</button>
</EditForm>

@code {
    private ServiceResultValidationEx<BootstrapFieldCssClassProvider>? validator;

    private async Task AddTodo()
    {
        validator?.ClearErrors();

        await this.ServiceSubmitAsync(
            handler: async () => await TodoService.AddAsync(newTodo),
            success: result => todos = result,
            error: errors => validator?.DisplayErrors(errors) ?? false
        );
    }
}
```

---

## Complete Examples

### Server-Side Service

```csharp
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Server;

[BlazorService]
public class TodoService : ITodoService
{
    private readonly TodoRepository _repository;

    public TodoService(TodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(TodoQueryModel query)
    {
        return await ServiceUtils.HandleServiceCallAsync(
            async (log) => await _repository.GetItemsAsync(query.Top, query.Page, query.Search)
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> AddAsync(TodoItem newTodo)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(newTodo.Title))
        {
            return ServiceActionResult<ModelList<TodoItem>?>.Failed(new ServiceActionError
            {
                ErrorType = ServiceActionErrorType.Validation,
                PropertyName = nameof(newTodo.Title),
                Message = "Title is required"
            });
        }

        // Check for duplicates
        if (await _repository.ExistsAsync(newTodo.Title))
        {
            return ServiceActionResult<ModelList<TodoItem>?>.Failed(new ServiceActionError
            {
                ErrorType = ServiceActionErrorType.Validation,
                PropertyName = nameof(newTodo.Title),
                Message = "A todo with this title already exists"
            });
        }

        return await ServiceUtils.HandleServiceCallAsync(
            async (log) => await _repository.AddAsync(newTodo)
        );
    }
}
```

### Client-Side Service (WASM)

```csharp
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.BlazorToolkit.Http;

[BlazorService]
public class TodoService : ITodoService
{
    private readonly IApiContext<TodoItem> _api;

    public TodoService(IApiContext<TodoItem> api)
    {
        _api = api;
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(TodoQueryModel query)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await _api.Get().Query(query).ExecuteListAsync()
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> AddAsync(TodoItem newTodo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await _api.Post(newTodo).ExecuteListAsync()
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> UpdateAsync(TodoItem todo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await _api.Put(todo, todo.Id).ExecuteListAsync()
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> DeleteAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (log) => await _api.Delete(id).ExecuteListAsync()
        );
    }
}
```

### Component

```razor
@page "/todos"
@inherits ServiceExecutionHostComponent
@inject ITodoService TodoService

<h1>Todo List</h1>

<ErrorMessageBanner IsError="@IsError" Message="@ErrorMessage" />

<EditForm Model="@newTodo" OnValidSubmit="@AddTodo">
    <ServiceResultValidationEx @ref="validator"
        CssProvider="new BootstrapFieldCssClassProvider()" />

    <div class="input-group mb-3">
        <InputText @bind-Value="newTodo.Title"
                   class="form-control"
                   placeholder="New todo..."
                   disabled="@InProgress" />
        <button type="submit" class="btn btn-primary" disabled="@InProgress">
            @if (IsSubmitting)
            {
                <span class="spinner-border spinner-border-sm"></span>
                <span>Adding...</span>
            }
            else
            {
                <span>Add</span>
            }
        </button>
    </div>
    <BoostrapValidationMessage For="@(() => newTodo.Title)" />
</EditForm>

@if (InProgress && ServiceState == ServiceExecutionType.Reading)
{
    <div class="text-center p-4">
        <div class="spinner-border"></div>
        <p>Loading todos...</p>
    </div>
}
else if (todos?.Items != null)
{
    <ul class="list-group">
        @foreach (var todo in todos.Items)
        {
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span class="@(todo.IsCompleted ? "text-decoration-line-through" : "")">
                    @todo.Title
                </span>
                <button class="btn btn-sm btn-danger"
                        @onclick="() => DeleteTodo(todo.Id)"
                        disabled="@InProgress">
                    Delete
                </button>
            </li>
        }
    </ul>

    <DataPager PagesCount="@todos.TotalPages"
               SelectedPage="@currentPage"
               OnPageChanged="@ChangePage"
               InProgress="@InProgress" />
}

@code {
    private ModelList<TodoItem>? todos;
    private TodoItem newTodo = new();
    private int currentPage = 0;
    private const int PageSize = 10;

    private ServiceResultValidationEx<BootstrapFieldCssClassProvider>? validator;

    private bool IsSubmitting => ServiceState == ServiceExecutionType.Submitting;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ChangePage(0);
    }

    private async Task ChangePage(int page)
    {
        currentPage = page;
        await this.ServiceReadAsync(
            handler: async () => await TodoService.GetItemsAsync(
                new TodoQueryModel { Top = PageSize, Page = page }
            ),
            success: result => todos = result,
            stateKey: nameof(todos)
        );
    }

    private async Task AddTodo()
    {
        validator?.ClearErrors();

        await this.ServiceSubmitAsync(
            handler: async () => await TodoService.AddAsync(newTodo),
            success: result =>
            {
                todos = result;
                newTodo = new TodoItem();
            },
            error: errors => validator?.DisplayErrors(errors) ?? false
        );
    }

    private async Task DeleteTodo(string id)
    {
        await this.ServiceSubmitAsync(
            handler: async () => await TodoService.DeleteAsync(id),
            success: result => todos = result
        );
    }
}
```

---

## API Reference

### Key Types

| Type | Namespace | Description |
|------|-----------|-------------|
| `IServiceExecutionHost` | `Services` | Interface for components handling service state |
| `ServiceExecutionHandler` | `Services` | Orchestrates service call execution |
| `ServiceActionResult<T>` | `Services` | Unified result wrapper |
| `ServiceActionError` | `Services` | Error information |
| `CallContext<T>` | `Services` | Parameter container for service calls |
| `IApiContext<T>` | `Http` | HTTP request builder interface |
| `HttpApiContextFactory` | `Http` | Creates API context instances |
| `ApiUrlBuilder` | `Http` | Fluent URL construction |
| `IObjectStore` | `Offline.Storage` | Generic IndexedDB key/value store |
| `IConnectivityService` | `Offline.Connectivity` | Tracks browser online/offline state |
| `ICacheableSource<T>` | `Offline.Sync` | Read-through local cache backed by the API |
| `IOutboxQueue` | `Offline.Sync` | Write-through sync queue with backoff |
| `IMasterDataSync` | `Offline.Sync` | Refreshes all registered cacheable sources |
| `CrudSyncHandler<T>` | `Offline.Sync` | Standard REST CRUD replay handler |

### Offline Extension Methods

| Method | Description |
|--------|-------------|
| `AddBlazorOffline()` | Register the IndexedDB store, connectivity, outbox, and master-data sync |
| `AddCacheableSource<T>()` | Register a read-through cache for an entity |
| `AddCrudSyncHandler<T>()` | Register a CRUD outbox handler for an entity |

### Extension Methods

| Method | Description |
|--------|-------------|
| `ServiceReadAsync<T>()` | Execute a read operation |
| `ServiceSubmitAsync<T>()` | Execute a submit operation |
| `BeginServiceCall()` | Start manual service execution |
| `SetException()` | Set error state from exception |
| `Top()`, `Page()`, `Search()`, `Sort()` | Query parameter helpers |
| `Query<T>()` | Serialize object to query parameters |

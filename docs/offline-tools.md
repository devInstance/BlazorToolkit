# BlazorToolkit Offline Tools (`DevInstance.BlazorToolkit.Offline`)

Reusable offline-first building blocks for Blazor WebAssembly apps: an IndexedDB
object store, connectivity tracking, a read-through cache, and a write-through
outbox (sync queue). Extracted from the Tentrie field client and generalized so any
app can declare its own stores and entities.

## Layers

| Namespace | Type | Role |
|---|---|---|
| `…Offline.Storage` | `IObjectStore` / `IndexedDbObjectStore` | Generic JSON key/value store over IndexedDB (`window.blazortoolkit.db.*`). |
| `…Offline.Storage` | `OfflineStoreOptions` | Declares the DB name/version and object stores (plus reserved `syncMeta`, `syncQueue`). |
| `…Offline.Connectivity` | `IConnectivityService` | `navigator.onLine` + online/offline events → `OnConnectivityChanged`. |
| `…Offline.Sync` | `ICacheableSource<T>` / `CacheableSource<T>` | Read-through cache: serve local immediately, refresh from the API in the background. |
| `…Offline.Sync` | `IOutboxQueue` / `OutboxProcessor` | Write-through queue with exponential backoff, `pending`/`failed`/`conflict` states. |
| `…Offline.Sync` | `ISyncOperationHandler`, `CrudSyncHandler<T>` | Per-entity-type replay logic. `CrudSyncHandler<T>` covers standard REST CRUD. |
| `…Offline.Sync` | `IMasterDataSync` / `MasterDataSync` | Refreshes all registered cacheable sources (on login / when stale). |

The server is expected to wrap list/item responses in
`ServiceActionResult<ModelList<T>>` / `ServiceActionResult<T>` — `CacheableSource`
unwraps that envelope.

## Wiring (Program.cs, WASM)

```csharp
builder.Services.AddBlazorOffline(opts =>
{
    opts.DatabaseName = "myapp";
    opts.DatabaseVersion = 1;
    opts.AddStore("leads");
    opts.AddStore("contacts");
});

// Read-through cache per entity
builder.Services.AddCacheableSource<LeadItem>(sp =>
{
    var store = sp.GetRequiredService<IObjectStore>();
    return new CacheableSourceOptions<LeadItem>
    {
        Endpoint = "leads",
        LoadLocal = () => store.GetAllAsync<LeadItem>("leads"),
        SaveLocal = items => store.ReplaceAllAsync("leads", items),
    };
});

// Outbox handler per entity (standard CRUD)
builder.Services.AddCrudSyncHandler<LeadItem>(sp =>
{
    var store = sp.GetRequiredService<IObjectStore>();
    return new CrudSyncHandlerOptions<LeadItem>
    {
        EntityType = "lead",
        Endpoint = "leads",
        LoadLocal = id => store.GetAsync<LeadItem>("leads", id),
    };
});
```

Then initialize once after `builder.Build()`:

```csharp
await host.Services.GetRequiredService<IObjectStore>().InitializeAsync();
await host.Services.GetRequiredService<IConnectivityService>().InitializeAsync();
```

## Static assets

The IndexedDB and connectivity JS ship as static web assets:

```html
<script src="_content/DevInstance.BlazorToolkit/js/blazortoolkit-db.js"></script>
<script src="_content/DevInstance.BlazorToolkit/js/blazortoolkit-connectivity.js"></script>
```

Load them **before** `blazor.webassembly.js` so the first interop call during host
init succeeds. TypeScript sources live in `src/Scripts/`.

## Feature service pattern

Inject `ICacheableSource<T>`, `IObjectStore`, and `IOutboxQueue`. Reads return the
cache immediately and fire a background refresh; writes save locally and enqueue:

```csharp
public async Task SaveAsync(T item, bool isNew)
{
    await store.PutAsync(storeName, item);
    await outbox.EnqueueAsync(entityType, item.Id, isNew ? "create" : "update");
}
```

To avoid a background refresh wiping an offline-created record before it syncs, make
the source's `SaveLocal` merge: keep local items that still have a pending outbox
entry and aren't on the server yet (see ThreadIQ's `OfflineCacheMerge.PreservePending`).

## Conflicts

`OutboxProcessor` parks an entry as `conflict` when a handler returns
`SyncOperationResult.Conflict(...)` (e.g. server state moved). Surface
`GetConflictsAsync()` in the UI and resolve with `DiscardAsync(entryId)`.
`CrudSyncHandler<T>` (last-write-wins) never produces conflicts; richer handlers can.

## Tests

`tests/DevInstance.BlazorToolkit.Offline.Tests` covers the outbox dispatch logic
(success / retry+backoff / conflict / drop / unknown-type / enqueue) with an
in-memory object store — no browser required.

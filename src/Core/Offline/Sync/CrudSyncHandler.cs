using DevInstance.BlazorToolkit.Http;
using DevInstance.LogScope;

namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Options for a <see cref="CrudSyncHandler{T}"/>: the entity type discriminator,
/// the REST endpoint, and how to load the locally-cached record for replay.
/// </summary>
public class CrudSyncHandlerOptions<T> where T : class
{
    /// <summary>Entity type discriminator (matches <see cref="SyncQueueEntry.EntityType"/>).</summary>
    public required string EntityType { get; init; }

    /// <summary>REST endpoint relative to the API base, e.g. "leads".</summary>
    public required string Endpoint { get; init; }

    /// <summary>Loads the locally-cached record by id for a create/update replay.</summary>
    public required Func<string, Task<T?>> LoadLocal { get; init; }
}

/// <summary>
/// A generic <see cref="ISyncOperationHandler"/> for entities that follow standard
/// REST CRUD (POST create / PUT update / DELETE) with last-write-wins semantics —
/// the common case for entities without a server-side workflow. Apps register one
/// per entity type instead of hand-writing a handler.
/// </summary>
public class CrudSyncHandler<T> : ISyncOperationHandler where T : class
{
    private readonly CrudSyncHandlerOptions<T> options;
    private readonly IHttpApiContextFactory? apiFactory;
    private readonly IScopeLog log;

    public string EntityType => options.EntityType;

    public CrudSyncHandler(
        CrudSyncHandlerOptions<T> options,
        IScopeManager scopeManager,
        IHttpApiContextFactory? apiFactory = null)
    {
        this.options = options;
        this.apiFactory = apiFactory;
        log = scopeManager.CreateLogger($"CrudSyncHandler<{typeof(T).Name}>");
    }

    public async Task<SyncOperationResult> ProcessAsync(SyncQueueEntry entry)
    {
        using var l = log.TraceScope();

        if (apiFactory == null)
        {
            l.D("No API factory — cannot replay (mock mode).");
            return SyncOperationResult.Retry("No API factory available.");
        }

        var api = apiFactory.CreateDefault<T>(options.Endpoint);

        try
        {
            // Delete needs no local record — deletes are enqueued after the local
            // copy is removed, so a lookup would (correctly) return null.
            if (entry.Operation == "delete")
            {
                await api.Delete(entry.EntityId).ExecuteAsync();
                l.I($"Deleted {options.Endpoint}/{entry.EntityId}");
                return SyncOperationResult.Ok();
            }

            var item = await options.LoadLocal(entry.EntityId);
            if (item == null)
            {
                l.D($"{options.Endpoint}/{entry.EntityId} not found locally — dropping.");
                return SyncOperationResult.Drop("Local record no longer exists.");
            }

            switch (entry.Operation)
            {
                case "create":
                    await api.Post(item).ExecuteAsync();
                    break;
                case "update":
                    await api.Put(item, entry.EntityId).ExecuteAsync();
                    break;
                default:
                    l.E($"Unknown operation '{entry.Operation}' — dropping entry {entry.Id}.");
                    return SyncOperationResult.Drop($"Unknown operation '{entry.Operation}'.");
            }

            l.I($"Synced {options.Endpoint}/{entry.EntityId} ({entry.Operation})");
            return SyncOperationResult.Ok();
        }
        catch (HttpRequestException ex)
        {
            l.E($"Sync failed for {options.Endpoint}/{entry.EntityId}: {ex.Message}");
            return SyncOperationResult.Retry(ex.Message);
        }
        catch (Exception ex)
        {
            l.E($"Unexpected error syncing {options.Endpoint}/{entry.EntityId}: {ex.Message}");
            return SyncOperationResult.Retry(ex.Message);
        }
    }
}

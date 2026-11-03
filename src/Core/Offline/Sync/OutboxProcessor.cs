using DevInstance.BlazorToolkit.Offline.Connectivity;
using DevInstance.BlazorToolkit.Offline.Storage;
using DevInstance.LogScope;
using DevInstance.WebServiceToolkit.Common.Tools;

namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Default <see cref="IOutboxQueue"/>. Persists entries in the reserved <c>syncQueue</c>
/// store, dispatches each to the matching <see cref="ISyncOperationHandler"/>, and applies
/// exponential backoff on transient failures. Auto-processes when connectivity is restored.
/// </summary>
public class OutboxProcessor : IOutboxQueue, IDisposable
{
    private readonly IObjectStore store;
    private readonly IReadOnlyDictionary<string, ISyncOperationHandler> handlers;
    private readonly IConnectivityService? connectivity;
    private readonly IScopeLog log;

    private bool isProcessing;
    private static readonly int[] BackoffSeconds = [0, 5, 15, 30, 60, 120, 300];

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Connected;
    public int PendingCount { get; private set; }
    public int ConflictCount { get; private set; }

    public event Action? OnStatusChanged;
    public event Action? OnProcessed;

    public OutboxProcessor(
        IObjectStore store,
        IEnumerable<ISyncOperationHandler> handlers,
        IScopeManager scopeManager,
        IConnectivityService? connectivity = null)
    {
        this.store = store;
        // Last registration wins per entity type, mirroring DI override semantics.
        this.handlers = handlers
            .GroupBy(h => h.EntityType)
            .ToDictionary(g => g.Key, g => g.Last());
        this.connectivity = connectivity;
        log = scopeManager.CreateLogger(this);

        if (connectivity != null)
            connectivity.OnConnectivityChanged += HandleConnectivityChanged;
    }

    public async Task EnqueueAsync(string entityType, string entityId, string operation, string? payload = null)
    {
        using var l = log.TraceScope();

        var entry = new SyncQueueEntry
        {
            Id = IdGenerator.New(),
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            Payload = payload,
            Status = "pending",
            CreatedAt = DateTime.Now,
            NextAttemptAt = DateTime.Now
        };

        await store.PutAsync(OfflineStoreOptions.QueueStore, entry);
        l.D($"Enqueued {operation} for {entityType} {entityId}");

        await RefreshCounts();
        _ = ProcessAsync();
    }

    public async Task ProcessAsync()
    {
        if (isProcessing) return;
        isProcessing = true;

        using var l = log.TraceScope();
        try
        {
            var entries = await GetByStatus("pending");
            if (entries.Count == 0)
            {
                SetStatus(ConnectionStatus.Connected);
                return;
            }

            SetStatus(ConnectionStatus.Syncing);
            var processedAny = false;

            foreach (var entry in entries.Where(e => e.NextAttemptAt <= DateTime.Now))
            {
                if (!handlers.TryGetValue(entry.EntityType, out var handler))
                {
                    l.E($"No handler for entity type '{entry.EntityType}' — dropping entry {entry.Id}");
                    await store.DeleteAsync(OfflineStoreOptions.QueueStore, entry.Id);
                    continue;
                }

                SyncOperationResult result;
                try
                {
                    result = await handler.ProcessAsync(entry);
                }
                catch (Exception ex)
                {
                    result = SyncOperationResult.Retry(ex.Message);
                }

                switch (result.Outcome)
                {
                    case SyncOperationOutcome.Success:
                        await store.DeleteAsync(OfflineStoreOptions.QueueStore, entry.Id);
                        processedAny = true;
                        break;

                    case SyncOperationOutcome.Drop:
                        l.D($"Dropping entry {entry.Id}: {result.Error}");
                        await store.DeleteAsync(OfflineStoreOptions.QueueStore, entry.Id);
                        break;

                    case SyncOperationOutcome.Conflict:
                        entry.Status = "conflict";
                        entry.LastError = result.Error;
                        await store.PutAsync(OfflineStoreOptions.QueueStore, entry);
                        l.I($"Entry {entry.Id} parked as conflict: {result.Error}");
                        break;

                    case SyncOperationOutcome.Retry:
                    default:
                        entry.RetryCount++;
                        entry.LastError = result.Error;
                        var idx = Math.Min(entry.RetryCount, BackoffSeconds.Length - 1);
                        entry.NextAttemptAt = DateTime.Now.AddSeconds(BackoffSeconds[idx]);
                        entry.Status = entry.RetryCount >= BackoffSeconds.Length ? "failed" : "pending";
                        await store.PutAsync(OfflineStoreOptions.QueueStore, entry);
                        break;
                }
            }

            if (processedAny)
                OnProcessed?.Invoke();

            await RefreshCounts();
            SetStatus(ConnectionStatus.Connected);
        }
        catch (Exception ex)
        {
            l.E($"Queue processing error: {ex.Message}");
            SetStatus(ConnectionStatus.Offline);
        }
        finally
        {
            isProcessing = false;
        }
    }

    public Task<List<SyncQueueEntry>> GetConflictsAsync() => GetByStatus("conflict");

    public async Task DiscardAsync(string entryId)
    {
        await store.DeleteAsync(OfflineStoreOptions.QueueStore, entryId);
        await RefreshCounts();
    }

    private Task<List<SyncQueueEntry>> GetByStatus(string status) =>
        store.GetByIndexAsync<SyncQueueEntry>(OfflineStoreOptions.QueueStore, "status", status);

    private async Task RefreshCounts()
    {
        PendingCount = (await GetByStatus("pending")).Count;
        ConflictCount = (await GetByStatus("conflict")).Count;
        OnStatusChanged?.Invoke();
    }

    private void HandleConnectivityChanged(bool online)
    {
        if (online) _ = ProcessAsync();
        else SetStatus(ConnectionStatus.Offline);
    }

    private void SetStatus(ConnectionStatus status)
    {
        if (Status == status) return;
        Status = status;
        OnStatusChanged?.Invoke();
    }

    public void Dispose()
    {
        if (connectivity != null)
            connectivity.OnConnectivityChanged -= HandleConnectivityChanged;
    }
}

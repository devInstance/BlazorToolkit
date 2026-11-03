using DevInstance.BlazorToolkit.Offline.Storage;
using DevInstance.LogScope;

namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Default <see cref="IMasterDataSync"/>. Refreshes every registered
/// <see cref="ICacheableSource"/> independently — one source failing does not block
/// the others — and records the last successful sync time in the <c>syncMeta</c> store.
/// </summary>
public class MasterDataSync : IMasterDataSync
{
    private const string LastSyncedKey = "lastSyncedAt";

    private readonly IEnumerable<ICacheableSource> sources;
    private readonly IObjectStore store;
    private readonly IScopeLog log;

    private bool isSyncing;

    public SyncState State { get; private set; } = SyncState.Idle;
    public DateTime? LastSyncedAt { get; private set; }
    public event Action? OnStateChanged;

    public MasterDataSync(
        IEnumerable<ICacheableSource> sources,
        IObjectStore store,
        IScopeManager scopeManager)
    {
        this.sources = sources;
        this.store = store;
        log = scopeManager.CreateLogger(this);
    }

    public async Task SyncAllAsync()
    {
        if (isSyncing) return;
        isSyncing = true;

        using var l = log.TraceScope();
        SetState(SyncState.Syncing);

        try
        {
            var errors = 0;
            foreach (var source in sources)
            {
                try
                {
                    var ok = await source.RefreshAsync();
                    if (!ok) errors++;
                }
                catch (Exception ex)
                {
                    errors++;
                    l.E($"Failed to sync {source.Name}: {ex.Message}");
                }
            }

            LastSyncedAt = DateTime.Now;
            await store.SetMetaAsync(LastSyncedKey, LastSyncedAt.Value.ToString("O"));
            SetState(errors > 0 ? SyncState.Failed : SyncState.Completed);
            l.I($"Master-data sync finished with {errors} error(s) at {LastSyncedAt:HH:mm:ss}");
        }
        catch (Exception ex)
        {
            l.E($"Master-data sync failed: {ex.Message}");
            SetState(SyncState.Failed);
        }
        finally
        {
            isSyncing = false;
        }
    }

    public async Task SyncIfStaleAsync(TimeSpan maxAge)
    {
        if (!LastSyncedAt.HasValue)
        {
            var stored = await store.GetMetaAsync(LastSyncedKey);
            if (stored != null && DateTime.TryParse(stored, out var parsed))
                LastSyncedAt = parsed;
        }

        if (LastSyncedAt.HasValue && DateTime.Now - LastSyncedAt.Value < maxAge)
            return;

        await SyncAllAsync();
    }

    private void SetState(SyncState state)
    {
        State = state;
        OnStateChanged?.Invoke();
    }
}

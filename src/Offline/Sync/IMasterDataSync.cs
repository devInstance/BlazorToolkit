namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Orchestrates pulling all registered <see cref="ICacheableSource"/> data into the
/// local cache. Typically invoked on login and periodically (or when stale).
/// </summary>
public interface IMasterDataSync
{
    /// <summary>Current sync state.</summary>
    SyncState State { get; }

    /// <summary>Last successful full-sync timestamp, or null if never synced.</summary>
    DateTime? LastSyncedAt { get; }

    /// <summary>Raised when <see cref="State"/> changes.</summary>
    event Action? OnStateChanged;

    /// <summary>Refreshes every registered cacheable source. Skips if already running.</summary>
    Task SyncAllAsync();

    /// <summary>Refreshes only if the last successful sync is older than <paramref name="maxAge"/>.</summary>
    Task SyncIfStaleAsync(TimeSpan maxAge);
}

/// <summary>State of a master-data sync pass.</summary>
public enum SyncState
{
    Idle,
    Syncing,
    Completed,
    Failed
}

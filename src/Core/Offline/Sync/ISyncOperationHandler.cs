namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Processes queued operations for one <see cref="EntityType"/>. The outbox processor
/// dispatches each <see cref="SyncQueueEntry"/> to the handler whose
/// <see cref="EntityType"/> matches, keeping the queue mechanics (backoff, status,
/// events) independent of any entity's transport details.
/// </summary>
public interface ISyncOperationHandler
{
    /// <summary>The entity type this handler services (matches <see cref="SyncQueueEntry.EntityType"/>).</summary>
    string EntityType { get; }

    /// <summary>Replays a single queued operation against the server.</summary>
    Task<SyncOperationResult> ProcessAsync(SyncQueueEntry entry);
}

/// <summary>Outcome of replaying a queued operation.</summary>
public enum SyncOperationOutcome
{
    /// <summary>Applied — remove from the queue.</summary>
    Success,
    /// <summary>Transient failure — keep and retry with backoff.</summary>
    Retry,
    /// <summary>Server state diverged — park as a conflict for user resolution.</summary>
    Conflict,
    /// <summary>Not applicable anymore (e.g. local record gone) — drop without retry.</summary>
    Drop
}

/// <summary>Result of <see cref="ISyncOperationHandler.ProcessAsync"/>.</summary>
public class SyncOperationResult
{
    public SyncOperationOutcome Outcome { get; init; }
    public string? Error { get; init; }

    public static SyncOperationResult Ok() => new() { Outcome = SyncOperationOutcome.Success };
    public static SyncOperationResult Retry(string? error = null) => new() { Outcome = SyncOperationOutcome.Retry, Error = error };
    public static SyncOperationResult Conflict(string? error = null) => new() { Outcome = SyncOperationOutcome.Conflict, Error = error };
    public static SyncOperationResult Drop(string? reason = null) => new() { Outcome = SyncOperationOutcome.Drop, Error = reason };
}

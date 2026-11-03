using DevInstance.BlazorToolkit.Offline.Connectivity;

namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// The outbound sync queue. Feature services enqueue create/update/delete operations
/// that are persisted locally and replayed against the server with exponential
/// backoff when connectivity allows.
/// </summary>
public interface IOutboxQueue
{
    /// <summary>Coarse connection status for UI.</summary>
    ConnectionStatus Status { get; }

    /// <summary>Number of entries still waiting to sync.</summary>
    int PendingCount { get; }

    /// <summary>Number of entries parked as conflicts awaiting user resolution.</summary>
    int ConflictCount { get; }

    /// <summary>Raised when status or the pending/conflict counts change.</summary>
    event Action? OnStatusChanged;

    /// <summary>Raised after the queue successfully applies one or more operations.</summary>
    event Action? OnProcessed;

    /// <summary>
    /// Enqueues an operation for later replay and kicks off a (fire-and-forget)
    /// processing pass.
    /// </summary>
    /// <param name="entityType">Discriminator selecting the handler (e.g. "lead").</param>
    /// <param name="entityId">Target entity id.</param>
    /// <param name="operation">"create", "update", "delete", or a handler-specific verb.</param>
    /// <param name="payload">Optional JSON payload for non-shape-preserving operations.</param>
    Task EnqueueAsync(string entityType, string entityId, string operation, string? payload = null);

    /// <summary>Processes all due pending entries. Safe to call repeatedly.</summary>
    Task ProcessAsync();

    /// <summary>Returns the entries currently parked as conflicts.</summary>
    Task<List<SyncQueueEntry>> GetConflictsAsync();

    /// <summary>Drops a conflicted entry without applying it.</summary>
    Task DiscardAsync(string entryId);
}

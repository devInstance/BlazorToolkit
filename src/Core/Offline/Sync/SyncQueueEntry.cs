namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// A queued outbound operation, persisted in the reserved IndexedDB <c>syncQueue</c>
/// store. The outbox processor replays these against the server when connectivity
/// allows, with exponential backoff on transient failures.
/// </summary>
public class SyncQueueEntry
{
    /// <summary>Unique queue-entry id.</summary>
    public string Id { get; set; } = "";

    /// <summary>The domain entity this operation targets (its PublicId / client id).</summary>
    public string EntityId { get; set; } = "";

    /// <summary>Entity type discriminator used to pick the handler, e.g. "lead", "contact".</summary>
    public string EntityType { get; set; } = "";

    /// <summary>Operation verb: "create", "update", "delete", or a handler-specific verb.</summary>
    public string Operation { get; set; } = "";

    /// <summary>Optional operation-specific payload, JSON-serialized. Null for shape-preserving
    /// create/update/delete operations that derive their data from the local record.</summary>
    public string? Payload { get; set; }

    /// <summary>Queue status: "pending", "failed" (retries exhausted), or "conflict"
    /// (terminal — needs user resolution).</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Number of retry attempts so far.</summary>
    public int RetryCount { get; set; }

    /// <summary>When the entry was queued.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Earliest time to attempt again (drives backoff).</summary>
    public DateTime NextAttemptAt { get; set; }

    /// <summary>Last error message, if any.</summary>
    public string? LastError { get; set; }
}

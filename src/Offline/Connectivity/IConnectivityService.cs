namespace DevInstance.BlazorToolkit.Offline.Connectivity;

/// <summary>
/// Tracks browser online/offline state via the <c>navigator.onLine</c> flag and
/// the window <c>online</c>/<c>offline</c> events. Unlike a purely inferred status
/// (success/failure of HTTP calls), this gives the outbox an explicit "we're back
/// online — process the queue now" trigger.
/// </summary>
public interface IConnectivityService
{
    /// <summary>Last known connectivity state. Defaults to <c>true</c> until initialized.</summary>
    bool IsOnline { get; }

    /// <summary>Raised when connectivity flips. Argument is the new online state.</summary>
    event Action<bool>? OnConnectivityChanged;

    /// <summary>Registers the JS listeners and reads the initial state. Idempotent.</summary>
    Task InitializeAsync();
}

/// <summary>Coarse connection state surfaced by the outbox for UI (sync chips, banners).</summary>
public enum ConnectionStatus
{
    Connected,
    Syncing,
    Offline
}

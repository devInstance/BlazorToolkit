using DevInstance.LogScope;
using Microsoft.JSInterop;

namespace DevInstance.BlazorToolkit.Offline.Connectivity;

/// <summary>
/// <see cref="IConnectivityService"/> backed by the toolkit's connectivity JS interop
/// (<c>window.blazortoolkit.connectivity.*</c>). Registers a <see cref="DotNetObjectReference{T}"/>
/// so the browser's online/offline events call back into managed code.
/// </summary>
public class ConnectivityService : IConnectivityService, IDisposable
{
    private readonly IJSRuntime js;
    private readonly IScopeLog log;
    private DotNetObjectReference<ConnectivityService>? selfRef;
    private bool initialized;

    public bool IsOnline { get; private set; } = true;

    public event Action<bool>? OnConnectivityChanged;

    public ConnectivityService(IJSRuntime js, IScopeManager scopeManager)
    {
        this.js = js;
        log = scopeManager.CreateLogger(this);
    }

    public async Task InitializeAsync()
    {
        if (initialized) return;
        initialized = true;

        using var l = log.TraceScope();
        selfRef = DotNetObjectReference.Create(this);
        try
        {
            IsOnline = await js.InvokeAsync<bool>("blazortoolkit.connectivity.initialize", selfRef);
            l.D($"Connectivity initialized; online={IsOnline}");
        }
        catch (Exception ex)
        {
            // Interop unavailable (e.g. prerender) — stay optimistic.
            l.D($"Connectivity init skipped: {ex.Message}");
        }
    }

    /// <summary>Invoked from JS when the browser fires an online/offline event.</summary>
    [JSInvokable]
    public void OnConnectivityChangedFromJs(bool online)
    {
        if (IsOnline == online) return;
        IsOnline = online;
        log.D($"Connectivity changed: online={online}");
        OnConnectivityChanged?.Invoke(online);
    }

    public void Dispose() => selfRef?.Dispose();
}

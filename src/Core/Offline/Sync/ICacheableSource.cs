namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Non-generic facet of a cacheable source so orchestrators (e.g.
/// <see cref="IMasterDataSync"/>) can refresh a heterogeneous set of sources
/// without knowing each entity type.
/// </summary>
public interface ICacheableSource
{
    /// <summary>A label for diagnostics — typically the server endpoint.</summary>
    string Name { get; }

    /// <summary>Pulls the latest data from the server into the local cache.</summary>
    Task<bool> RefreshAsync();
}

/// <summary>
/// A locally-cached, server-backed collection of <typeparamref name="T"/>.
/// Reads come from the local store immediately; <see cref="RefreshAsync"/> pulls
/// fresh data from the API in the background and raises <see cref="OnCacheUpdated"/>.
/// </summary>
public interface ICacheableSource<T> : ICacheableSource where T : class
{
    /// <summary>Returns the locally cached items (no network call).</summary>
    Task<List<T>> GetCachedAsync();

    /// <summary>True while a refresh is in flight.</summary>
    bool IsRefreshing { get; }

    /// <summary>Raised after a successful refresh writes new data to the cache.</summary>
    event Action? OnCacheUpdated;

    /// <summary>Raised when <see cref="IsRefreshing"/> changes.</summary>
    event Action? OnRefreshStateChanged;
}

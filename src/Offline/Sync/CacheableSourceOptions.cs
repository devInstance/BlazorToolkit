namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Configures a <see cref="CacheableSource{T}"/>: where to pull from on the server
/// and how to read/write the local cache. The local store specifics are supplied as
/// delegates so the cache layer stays decoupled from any particular storage shape.
/// </summary>
public class CacheableSourceOptions<T> where T : class
{
    /// <summary>Server endpoint (relative to the API base), e.g. "leads".</summary>
    public required string Endpoint { get; init; }

    /// <summary>Loads the cached items from local storage.</summary>
    public required Func<Task<List<T>>> LoadLocal { get; init; }

    /// <summary>Persists the freshly-pulled items to local storage.</summary>
    public required Func<List<T>, Task> SaveLocal { get; init; }

    /// <summary>Optional cap on the number of cached items (newest kept).</summary>
    public int? MaxCount { get; init; }

    /// <summary>Optional max age; items older than this are dropped from the cache.</summary>
    public TimeSpan? MaxAge { get; init; }

    /// <summary>Selects the update timestamp used by <see cref="MaxCount"/>/<see cref="MaxAge"/>.</summary>
    public Func<T, DateTime>? UpdateDateSelector { get; init; }

    /// <summary>
    /// Page size requested on refresh. Cacheable entities are meant to be fully
    /// mirrored locally, so this defaults large enough to fetch all rows at once.
    /// </summary>
    public int PageSize { get; init; } = 1000;
}

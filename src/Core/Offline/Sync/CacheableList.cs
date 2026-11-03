namespace DevInstance.BlazorToolkit.Offline.Sync;

/// <summary>
/// Wire shape a <see cref="CacheableSource{T}"/> reads from a list endpoint. Only
/// <see cref="Items"/> is needed to fill the cache; paging fields in the response
/// (<c>TotalCount</c>, <c>Page</c>, ...) are ignored during deserialization, so any
/// <c>IModelList&lt;T&gt;</c>-shaped payload is accepted.
/// </summary>
internal sealed class CacheableList<T>
{
    public T[]? Items { get; set; }
}

namespace DevInstance.BlazorToolkit.Offline.Storage;

/// <summary>
/// Generic key/value object store backed by IndexedDB. Records are serialized to
/// JSON and addressed by store name + key. This is the offline persistence
/// primitive the cache and outbox layers build on; applications typically wrap it
/// in a typed facade per entity.
/// </summary>
public interface IObjectStore
{
    /// <summary>Opens the database, creating stores/indexes per the configured options.</summary>
    Task InitializeAsync();

    /// <summary>Gets a single record by key, or default if not present.</summary>
    Task<T?> GetAsync<T>(string store, string id);

    /// <summary>Gets all records in a store.</summary>
    Task<List<T>> GetAllAsync<T>(string store);

    /// <summary>Gets all records whose secondary index matches <paramref name="value"/>.</summary>
    Task<List<T>> GetByIndexAsync<T>(string store, string indexName, object value);

    /// <summary>Inserts or replaces a record (keyed by the store's keyPath).</summary>
    Task PutAsync<T>(string store, T record);

    /// <summary>Deletes a record by key. No-op if absent.</summary>
    Task DeleteAsync(string store, string id);

    /// <summary>Clears a store and writes the given records in one transaction.</summary>
    Task ReplaceAllAsync<T>(string store, IEnumerable<T> records);

    /// <summary>Clears every record in a single store.</summary>
    Task ClearAsync(string store);

    /// <summary>Clears every store in the database.</summary>
    Task ClearAllAsync();

    /// <summary>Reads a value from the reserved <c>syncMeta</c> store, or null.</summary>
    Task<string?> GetMetaAsync(string key);

    /// <summary>Writes a value to the reserved <c>syncMeta</c> store.</summary>
    Task SetMetaAsync(string key, string value);
}

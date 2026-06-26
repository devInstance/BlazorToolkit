using DevInstance.BlazorToolkit.Offline.Storage;
using DevInstance.BlazorToolkit.Offline.Sync;

namespace DevInstance.BlazorToolkit.Offline.Tests;

/// <summary>
/// In-memory <see cref="IObjectStore"/> for tests. Keys records by their <c>Id</c>
/// property and supports index queries by a named property (used for the queue's
/// "status" index). No JS / IndexedDB involved.
/// </summary>
public class InMemoryObjectStore : IObjectStore
{
    private readonly Dictionary<string, Dictionary<string, object>> stores = new();
    private readonly Dictionary<string, string> meta = new();

    private Dictionary<string, object> Store(string name) =>
        stores.TryGetValue(name, out var s) ? s : stores[name] = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task<T?> GetAsync<T>(string store, string id) =>
        Task.FromResult(Store(store).TryGetValue(id, out var v) ? (T?)v : default);

    public Task<List<T>> GetAllAsync<T>(string store) =>
        Task.FromResult(Store(store).Values.Cast<T>().ToList());

    public Task<List<T>> GetByIndexAsync<T>(string store, string indexName, object value)
    {
        var prop = typeof(T).GetProperty(indexName,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var result = Store(store).Values.Cast<T>()
            .Where(x => Equals(prop?.GetValue(x)?.ToString(), value?.ToString()))
            .ToList();
        return Task.FromResult(result);
    }

    public Task PutAsync<T>(string store, T record)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(record)?.ToString()
                 ?? throw new InvalidOperationException("Record has no Id.");
        Store(store)[id] = record!;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string store, string id)
    {
        Store(store).Remove(id);
        return Task.CompletedTask;
    }

    public Task ReplaceAllAsync<T>(string store, IEnumerable<T> records)
    {
        var s = Store(store);
        s.Clear();
        foreach (var r in records)
        {
            var id = typeof(T).GetProperty("Id")?.GetValue(r)?.ToString()!;
            s[id] = r!;
        }
        return Task.CompletedTask;
    }

    public Task ClearAsync(string store) { Store(store).Clear(); return Task.CompletedTask; }
    public Task ClearAllAsync() { stores.Clear(); return Task.CompletedTask; }
    public Task<string?> GetMetaAsync(string key) => Task.FromResult(meta.TryGetValue(key, out var v) ? v : null);
    public Task SetMetaAsync(string key, string value) { meta[key] = value; return Task.CompletedTask; }
}

/// <summary>Handler that returns a scripted outcome and records how many times it ran.</summary>
public class ScriptedHandler : ISyncOperationHandler
{
    private readonly Func<SyncQueueEntry, SyncOperationResult> respond;
    public string EntityType { get; }
    public int Calls { get; private set; }

    public ScriptedHandler(string entityType, Func<SyncQueueEntry, SyncOperationResult> respond)
    {
        EntityType = entityType;
        this.respond = respond;
    }

    public Task<SyncOperationResult> ProcessAsync(SyncQueueEntry entry)
    {
        Calls++;
        return Task.FromResult(respond(entry));
    }
}

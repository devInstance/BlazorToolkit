using System.Text.Json;
using DevInstance.LogScope;
using Microsoft.JSInterop;

namespace DevInstance.BlazorToolkit.Offline.Storage;

/// <summary>
/// <see cref="IObjectStore"/> implementation that calls the toolkit's IndexedDB
/// JS interop (<c>window.blazortoolkit.db.*</c>, shipped as a static web asset).
/// All records cross the interop boundary as JSON strings.
/// </summary>
public class IndexedDbObjectStore : IObjectStore
{
    private readonly IJSRuntime js;
    private readonly OfflineStoreOptions options;
    private readonly IScopeLog log;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public IndexedDbObjectStore(IJSRuntime js, OfflineStoreOptions options, IScopeManager scopeManager)
    {
        this.js = js;
        this.options = options;
        log = scopeManager.CreateLogger(this);
    }

    public async Task InitializeAsync()
    {
        using var l = log.TraceScope();
        // Hand the store schema to the JS layer so it can create object stores and
        // indexes during onupgradeneeded. Serialized so the JS side stays dumb.
        var config = JsonSerializer.Serialize(new
        {
            name = options.DatabaseName,
            version = options.DatabaseVersion,
            stores = options.Stores.Select(s => new
            {
                name = s.Name,
                keyPath = s.KeyPath,
                indexes = s.Indexes.Select(i => new { name = i.Name, keyPath = i.KeyPath, unique = i.Unique })
            })
        }, JsonOptions);

        await js.InvokeVoidAsync("blazortoolkit.db.open", config);
        l.D($"IndexedDB '{options.DatabaseName}' v{options.DatabaseVersion} initialized with {options.Stores.Count} store(s)");
    }

    public async Task<T?> GetAsync<T>(string store, string id)
    {
        var json = await js.InvokeAsync<string?>("blazortoolkit.db.get", store, id);
        return json != null ? JsonSerializer.Deserialize<T>(json, JsonOptions) : default;
    }

    public async Task<List<T>> GetAllAsync<T>(string store)
    {
        var json = await js.InvokeAsync<string>("blazortoolkit.db.getAll", store);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new();
    }

    public async Task<List<T>> GetByIndexAsync<T>(string store, string indexName, object value)
    {
        var json = await js.InvokeAsync<string>("blazortoolkit.db.getByIndex", store, indexName, value);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new();
    }

    public async Task PutAsync<T>(string store, T record)
    {
        var json = JsonSerializer.Serialize(record, JsonOptions);
        await js.InvokeVoidAsync("blazortoolkit.db.put", store, json);
    }

    public Task DeleteAsync(string store, string id) =>
        js.InvokeVoidAsync("blazortoolkit.db.delete", store, id).AsTask();

    public async Task ReplaceAllAsync<T>(string store, IEnumerable<T> records)
    {
        var json = JsonSerializer.Serialize(records, JsonOptions);
        await js.InvokeVoidAsync("blazortoolkit.db.replaceAll", store, json);
    }

    public Task ClearAsync(string store) =>
        js.InvokeVoidAsync("blazortoolkit.db.clear", store).AsTask();

    public async Task ClearAllAsync()
    {
        using var l = log.TraceScope();
        await js.InvokeVoidAsync("blazortoolkit.db.clearAll");
        l.D("All stores cleared");
    }

    public async Task<string?> GetMetaAsync(string key)
    {
        var json = await js.InvokeAsync<string?>("blazortoolkit.db.get", OfflineStoreOptions.MetaStore, key);
        if (json == null) return null;
        var record = JsonSerializer.Deserialize<MetaRecord>(json, JsonOptions);
        return record?.Value;
    }

    public async Task SetMetaAsync(string key, string value)
    {
        var json = JsonSerializer.Serialize(new MetaRecord { Key = key, Value = value }, JsonOptions);
        await js.InvokeVoidAsync("blazortoolkit.db.put", OfflineStoreOptions.MetaStore, json);
    }

    private class MetaRecord
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}

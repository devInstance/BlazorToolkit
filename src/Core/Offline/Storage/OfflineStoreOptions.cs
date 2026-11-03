namespace DevInstance.BlazorToolkit.Offline.Storage;

/// <summary>
/// Declares the IndexedDB database an offline-capable app uses: its name, version,
/// and the object stores to create. Two stores are always present and reserved by
/// the toolkit: <c>syncMeta</c> (key/value sync bookkeeping) and <c>syncQueue</c>
/// (the outbound operation queue). Apps add their own entity stores via
/// <see cref="AddStore"/>.
/// </summary>
public class OfflineStoreOptions
{
    /// <summary>The reserved key/value metadata store name.</summary>
    public const string MetaStore = "syncMeta";

    /// <summary>The reserved outbound sync-queue store name.</summary>
    public const string QueueStore = "syncQueue";

    /// <summary>IndexedDB database name. Bump <see cref="DatabaseVersion"/> when stores change.</summary>
    public string DatabaseName { get; set; } = "blazortoolkit";

    /// <summary>
    /// IndexedDB schema version. Increment whenever the set of stores or indexes
    /// changes so the browser runs an upgrade and creates the new stores.
    /// </summary>
    public int DatabaseVersion { get; set; } = 1;

    /// <summary>The application-declared stores, plus the two reserved ones.</summary>
    public List<OfflineStoreDefinition> Stores { get; } = new()
    {
        new OfflineStoreDefinition { Name = MetaStore, KeyPath = "key" },
        new OfflineStoreDefinition
        {
            Name = QueueStore,
            KeyPath = "id",
            Indexes = { new OfflineStoreIndex { Name = "status", KeyPath = "status" } }
        },
    };

    /// <summary>
    /// Declares an application object store. Call once per entity type that is
    /// cached or queued locally.
    /// </summary>
    /// <param name="name">Store name (e.g. "leads").</param>
    /// <param name="keyPath">Primary key property name. Defaults to "id".</param>
    /// <param name="indexes">Optional secondary indexes.</param>
    public OfflineStoreOptions AddStore(string name, string keyPath = "id", params OfflineStoreIndex[] indexes)
    {
        Stores.Add(new OfflineStoreDefinition
        {
            Name = name,
            KeyPath = keyPath,
            Indexes = indexes.ToList()
        });
        return this;
    }
}

/// <summary>An IndexedDB object store definition.</summary>
public class OfflineStoreDefinition
{
    public required string Name { get; init; }
    public string KeyPath { get; init; } = "id";
    public List<OfflineStoreIndex> Indexes { get; init; } = new();
}

/// <summary>A secondary index on an object store.</summary>
public class OfflineStoreIndex
{
    public required string Name { get; init; }
    public required string KeyPath { get; init; }
    public bool Unique { get; init; }
}

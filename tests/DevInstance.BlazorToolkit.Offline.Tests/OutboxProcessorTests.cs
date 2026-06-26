using DevInstance.BlazorToolkit.Offline.Storage;
using DevInstance.BlazorToolkit.Offline.Sync;
using Xunit;

namespace DevInstance.BlazorToolkit.Offline.Tests;

public class OutboxProcessorTests
{
    private static SyncQueueEntry Entry(string type, string op = "create", string status = "pending") => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        EntityType = type,
        EntityId = "e1",
        Operation = op,
        Status = status,
        CreatedAt = DateTime.Now,
        NextAttemptAt = DateTime.Now,
    };

    private static OutboxProcessor Build(InMemoryObjectStore store, params ISyncOperationHandler[] handlers) =>
        new(store, handlers, new ScopeManagerMock(), connectivity: null);

    private static Task SeedAsync(InMemoryObjectStore store, SyncQueueEntry entry) =>
        store.PutAsync(OfflineStoreOptions.QueueStore, entry);

    private static Task<List<SyncQueueEntry>> PendingAsync(InMemoryObjectStore store) =>
        store.GetByIndexAsync<SyncQueueEntry>(OfflineStoreOptions.QueueStore, "status", "pending");

    [Fact]
    public async Task Success_removes_entry_and_raises_processed()
    {
        var store = new InMemoryObjectStore();
        var handler = new ScriptedHandler("lead", _ => SyncOperationResult.Ok());
        var processor = Build(store, handler);
        await SeedAsync(store, Entry("lead"));

        var processed = false;
        processor.OnProcessed += () => processed = true;

        await processor.ProcessAsync();

        Assert.Equal(1, handler.Calls);
        Assert.Empty(await PendingAsync(store));
        Assert.True(processed);
        Assert.Equal(0, processor.PendingCount);
    }

    [Fact]
    public async Task Retry_keeps_entry_pending_with_backoff()
    {
        var store = new InMemoryObjectStore();
        var handler = new ScriptedHandler("lead", _ => SyncOperationResult.Retry("boom"));
        var processor = Build(store, handler);
        await SeedAsync(store, Entry("lead"));

        await processor.ProcessAsync();

        var pending = await PendingAsync(store);
        Assert.Single(pending);
        Assert.Equal(1, pending[0].RetryCount);
        Assert.Equal("boom", pending[0].LastError);
        Assert.True(pending[0].NextAttemptAt > DateTime.Now); // backed off

        // A second immediate pass must not re-run the handler — the entry isn't due yet.
        await processor.ProcessAsync();
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Conflict_parks_entry_for_resolution()
    {
        var store = new InMemoryObjectStore();
        var handler = new ScriptedHandler("lead", _ => SyncOperationResult.Conflict("moved"));
        var processor = Build(store, handler);
        await SeedAsync(store, Entry("lead"));

        await processor.ProcessAsync();

        Assert.Empty(await PendingAsync(store));
        var conflicts = await processor.GetConflictsAsync();
        Assert.Single(conflicts);
        Assert.Equal("moved", conflicts[0].LastError);
        Assert.Equal(1, processor.ConflictCount);
    }

    [Fact]
    public async Task Drop_removes_entry_without_retry()
    {
        var store = new InMemoryObjectStore();
        var handler = new ScriptedHandler("lead", _ => SyncOperationResult.Drop("gone"));
        var processor = Build(store, handler);
        await SeedAsync(store, Entry("lead"));

        await processor.ProcessAsync();

        Assert.Equal(1, handler.Calls);
        Assert.Empty(await PendingAsync(store));
        Assert.Empty(await processor.GetConflictsAsync());
    }

    [Fact]
    public async Task Unknown_entity_type_is_dropped()
    {
        var store = new InMemoryObjectStore();
        var processor = Build(store, new ScriptedHandler("lead", _ => SyncOperationResult.Ok()));
        await SeedAsync(store, Entry("ghost"));

        await processor.ProcessAsync();

        Assert.Empty(await PendingAsync(store));
    }

    [Fact]
    public async Task Enqueue_persists_a_pending_entry()
    {
        var store = new InMemoryObjectStore();
        // Handler retries so the entry stays pending regardless of the auto-process pass.
        var processor = Build(store, new ScriptedHandler("lead", _ => SyncOperationResult.Retry()));

        await processor.EnqueueAsync("lead", "e1", "create");

        var pending = await PendingAsync(store);
        Assert.Single(pending);
        Assert.Equal("lead", pending[0].EntityType);
        Assert.Equal("create", pending[0].Operation);
    }
}

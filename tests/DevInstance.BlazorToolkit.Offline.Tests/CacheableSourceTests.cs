using System.Net;
using System.Text;
using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Offline.Sync;
using Xunit;

namespace DevInstance.BlazorToolkit.Offline.Tests;

public class CacheableSourceTests
{
    public class Item
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    private sealed class StubHandler(HttpStatusCode status, string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }

    private sealed class StubClientFactory(HttpStatusCode status, string json) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(new StubHandler(status, json)) { BaseAddress = new Uri("http://localhost/") };
    }

    private sealed class Cache
    {
        public List<Item> Items { get; set; } = new() { new Item { Id = "old", Name = "Cached" } };
    }

    private static CacheableSource<Item> Source(Cache cache, HttpStatusCode status, string json) =>
        new(new CacheableSourceOptions<Item>
            {
                Endpoint = "items",
                LoadLocal = () => Task.FromResult(cache.Items.ToList()),
                SaveLocal = items => { cache.Items = items; return Task.CompletedTask; }
            },
            new ScopeManagerMock(),
            new HttpApiContextFactory(new StubClientFactory(status, json), "test", "api"));

    [Fact]
    public async Task RefreshAsync_BareList_ReplacesCache()
    {
        var cache = new Cache();
        var json = """{"totalCount":2,"pagesCount":1,"page":0,"count":2,"items":[{"id":"1","name":"A"},{"id":"2","name":"B"}]}""";

        var ok = await Source(cache, HttpStatusCode.OK, json).RefreshAsync();

        Assert.True(ok);
        Assert.Equal(new[] { "A", "B" }, cache.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task RefreshAsync_EmptyBareList_ClearsCache()
    {
        var cache = new Cache();

        var ok = await Source(cache, HttpStatusCode.OK, """{"totalCount":0,"items":[]}""").RefreshAsync();

        Assert.True(ok);
        Assert.Empty(cache.Items);
    }

    [Fact]
    public async Task RefreshAsync_EnvelopePayload_LeavesCacheUntouched()
    {
        // A server still wrapping lists in ServiceActionResult has no top-level "items";
        // that must not be read as an empty list, or the refresh would wipe the cache.
        var cache = new Cache();
        var json = """{"success":true,"isAuthorized":true,"result":{"totalCount":1,"items":[{"id":"1","name":"A"}]}}""";

        var ok = await Source(cache, HttpStatusCode.OK, json).RefreshAsync();

        Assert.False(ok);
        Assert.Equal("Cached", Assert.Single(cache.Items).Name);
    }

    [Fact]
    public async Task RefreshAsync_ServerError_LeavesCacheUntouched()
    {
        var cache = new Cache();

        var ok = await Source(cache, HttpStatusCode.InternalServerError, """{"errorType":1,"message":"boom"}""").RefreshAsync();

        Assert.False(ok);
        Assert.Equal("Cached", Assert.Single(cache.Items).Name);
    }
}

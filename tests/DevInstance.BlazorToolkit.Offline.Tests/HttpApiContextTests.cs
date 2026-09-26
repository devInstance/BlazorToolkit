using System.Net;
using System.Text;
using DevInstance.BlazorToolkit.Exceptions;
using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Services;
using Xunit;

namespace DevInstance.BlazorToolkit.Offline.Tests;

public class HttpApiContextTests
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

    private static IApiContext<string, Item> Api(HttpStatusCode status, string json) =>
        new HttpApiContextFactory(null!).Create<string, Item>(
            new HttpClient(new StubHandler(status, json)) { BaseAddress = new Uri("http://localhost/") }, "api/items");

    [Fact]
    public async Task ExecuteModelListAsync_DeserializesListResponse()
    {
        var json = """{"totalCount":5,"pagesCount":3,"page":1,"count":2,"sortOrder":["+name"],"search":"a","items":[{"id":"1","name":"A"},{"id":"2","name":"B"}]}""";

        var list = await Api(HttpStatusCode.OK, json).Get().ExecuteModelListAsync();

        Assert.NotNull(list);
        Assert.Equal(5, list.TotalCount);
        Assert.Equal(3, list.PagesCount);
        Assert.Equal(1, list.Page);
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "+name" }, list.SortOrder);
        Assert.Equal("a", list.Search);
        Assert.Equal(new[] { "A", "B" }, list.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task Get_NonSuccessWithErrorBody_ThrowsHttpServerExceptionWithServerError()
    {
        // WebServiceToolkit's WebServiceError body: errorType 2 = General.
        var json = """{"errorType":2,"message":"Item not found","propertyName":null}""";

        var ex = await Assert.ThrowsAsync<HttpServerException>(() => Api(HttpStatusCode.NotFound, json).Get("1").ExecuteAsync());

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal(ServiceActionErrorType.General, ex.Error.ErrorType);
        Assert.Equal("Item not found", ex.Error.Message);
    }
}

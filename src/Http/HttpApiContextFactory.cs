
using System.Net.Http;

namespace DevInstance.BlazorToolkit.Http;

public class HttpApiContextFactory
{
    private readonly IHttpClientFactory httpFactory;

    public HttpApiContextFactory(IHttpClientFactory factory)
    {
        httpFactory = factory;
    }

    public IApiContext<T> Create<T>(string clientName, string url)
    {
        return new HttpApiContext<T>(url, httpFactory.CreateClient(clientName));
    }

    public static IApiContext<T> Create<T>(HttpClient http, string url)
    {
        return new HttpApiContext<T>(url, http);
    }
}

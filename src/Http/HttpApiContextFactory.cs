
using System.Net.Http;

namespace DevInstance.BlazorToolkit.Http;

/// <summary>
/// Factory class for creating instances of <see cref="IApiContext{T}"/>.
/// </summary>
public class HttpApiContextFactory
{
    private readonly IHttpClientFactory httpFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpApiContextFactory"/> class.
    /// </summary>
    /// <param name="factory">The HTTP client factory.</param>
    public HttpApiContextFactory(IHttpClientFactory factory)
    {
        httpFactory = factory;
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified client name and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="clientName">The name of the HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public IApiContext<T> Create<T>(string clientName, string url)
    {
        return new HttpApiContext<T>(url, httpFactory.CreateClient(clientName));
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified HTTP client and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="http">The HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public static IApiContext<T> Create<T>(HttpClient http, string url)
    {
        return new HttpApiContext<T>(url, http);
    }
}

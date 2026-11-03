
using System;
using System.Net.Http;

namespace DevInstance.BlazorToolkit.Http;

/// <summary>
/// Factory class for creating instances of <see cref="IApiContext{T}"/>.
/// </summary>
public class HttpApiContextFactory : IHttpApiContextFactory
{
    private readonly IHttpClientFactory httpFactory;
    private readonly string clientName;
    private readonly string baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpApiContextFactory"/> class.
    /// </summary>
    /// <param name="factory">The HTTP client factory.</param>
    /// <param name="clientName">The name of the HTTP client. Default is "BlazorToolkitClient".</param>
    /// <param name="baseUrl">The base URL for the API context. Default is null.</param>
    public HttpApiContextFactory(IHttpClientFactory factory, string clientName = "BlazorToolkitClient", string baseUrl = null)
    {
        httpFactory = factory;
        this.clientName = clientName;
        this.baseUrl = baseUrl;
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified client name and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="clientName">The name of the HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public IApiContext<K, T> Create<K, T>(string clientName, string url)
    {
        return new HttpApiContext<K, T>(url, httpFactory.CreateClient(clientName));
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified HTTP client and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="http">The HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public IApiContext<K, T> Create<K, T>(HttpClient http, string url)
    {
        return new HttpApiContext<K, T>(url, http);
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
    public IApiContext<T> Create<T>(HttpClient http, string url)
    {
        return new HttpApiContext<T>(url, http);
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using default parameters.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public IApiContext<K, T> CreateDefault<K, T>(string url)
    {
        return new HttpApiContext<K, T>(ApiUrlBuilder.Create(baseUrl).Path(url).ToString(), httpFactory.CreateClient(clientName));
    }

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using default parameters.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="url">The URL path to append to the base URL.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    public IApiContext<T> CreateDefault<T>(string url)
    {
        return new HttpApiContext<T>(ApiUrlBuilder.Create(baseUrl).Path(url).ToString(), httpFactory.CreateClient(clientName));
    }
}

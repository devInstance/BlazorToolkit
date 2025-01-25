using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Http;

public interface IHttpApiContextFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified client name and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="clientName">The name of the HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    IApiContext<T> Create<T>(string clientName, string url);

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using the specified HTTP client and URL.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="http">The HTTP client.</param>
    /// <param name="url">The URL for the API context.</param>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    IApiContext<T> Create<T>(HttpClient http, string url);

    /// <summary>
    /// Creates an instance of <see cref="IApiContext{T}"/> using default parameters.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>An instance of <see cref="IApiContext{T}"/>.</returns>
    IApiContext<T> CreateDefault<T>(string baseUrl);
}

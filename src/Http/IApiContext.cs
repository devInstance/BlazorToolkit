using DevInstance.WebServiceToolkit.Common.Model;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Http;

public interface IApiContext : IApiContext<object> { }

public interface IApiContext<T> : IApiContext<string, T>  {
    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Get(string? id = default);

    /// <summary>
    /// Posts a new entity.
    /// </summary>
    /// <param name="obj">The entity to post.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Post(T obj);
    IApiContext<T> Post<O>(O obj);

    /// <summary>
    /// Puts (updates) an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="obj">The entity to update.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Put(T obj, string? id = default);
    IApiContext<T> Put<O>(O obj, string? id = default);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Delete(string? id = default);

    IApiContext<T> Url(string url);

    IApiContext<T> Url(ApiUrlBuilder url);

    /// <summary>
    /// Adds a parameter to the query string of the url.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IApiContext<T> Parameter<F>(string name, F value);
    IApiContext<T> Path(string name);
    IApiContext<T> Fragment(string name);
}

/// <summary>
/// Interface for API context operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IApiContext<K, T>
{
    /// <summary>
    /// Gets the constructed URI of the API context.
    /// </summary>
    string Uri { get; }

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<K, T> Get(K? id = default);

    /// <summary>
    /// Posts a new entity.
    /// </summary>
    /// <param name="obj">The entity to post.</param>
    /// <returns>The API context.</returns>
    IApiContext<K, T> Post(T obj);
    IApiContext<K, T> Post<O>(O obj);

    /// <summary>
    /// Puts (updates) an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="obj">The entity to update.</param>
    /// <returns>The API context.</returns>
    IApiContext<K, T> Put(T obj, K? id = default);
    IApiContext<K, T> Put<O>(O obj, K? id = default);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<K, T> Delete(K? id = default);

    IApiContext<K, T> Url(string url);

    IApiContext<K, T> Url(ApiUrlBuilder url);

    /// <summary>
    /// Adds a parameter to the query string of the url.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IApiContext<K, T> Parameter<F>(string name, F value);
    IApiContext<K, T> Path(string name);
    IApiContext<K, T> Fragment(string name);

    /// <summary>
    /// Executes the API context operation asynchronously.
    /// </summary>
    /// <returns>The entity.</returns>
    Task<O?> ExecuteAsync<O>();

    /// <summary>
    /// Executes the API context operation asynchronously.
    /// </summary>
    /// <returns>The entity.</returns>
    Task<T?> ExecuteAsync();

    /// <summary>
    /// Lists the entities asynchronously.
    /// </summary>
    /// <returns>A list of entities.</returns>
    Task<ModelList<T>?> ExecuteListAsync();
}

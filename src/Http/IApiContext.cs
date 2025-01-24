using DevInstance.WebServiceToolkit.Common.Model;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Http;

/// <summary>
/// Interface for API context operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IApiContext<T>
{
    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Get(string? id = null);

    /// <summary>
    /// Posts a new entity.
    /// </summary>
    /// <param name="obj">The entity to post.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Post(T obj);

    /// <summary>
    /// Posts a new entity.
    /// </summary>
    /// <param name="obj">The entity to post.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Post(object obj);

    /// <summary>
    /// Puts (updates) an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="obj">The entity to update.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Put(string? id, T obj);

    /// <summary>
    /// Puts (updates) an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="obj">The entity to update.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Put(string? id, object obj);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Delete(string? id);

    IApiContext<T> Url(string url);

    IApiContext<T> Url(ApiUrlBuilder url);

    /// <summary>
    /// Adds a parameter to the query string of the url.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IApiContext<T> Parameter(string name, object value);
    IApiContext<T> Parameter<F>(string name, F value);
    IApiContext<T> Path(string name);
    IApiContext<T> Fragment(string name);

    /// <summary>
    /// Executes the API context operation asynchronously.
    /// </summary>
    /// <returns>The entity.</returns>
    Task<object> ExecuteAsync();

    /// <summary>
    /// Executes the API context operation asynchronously.
    /// </summary>
    /// <returns>The entity.</returns>
    Task<T?> ExecuteItemAsync();

    /// <summary>
    /// Lists the entities asynchronously.
    /// </summary>
    /// <returns>A list of entities.</returns>
    Task<ModelList<T>?> ListAsync();
}

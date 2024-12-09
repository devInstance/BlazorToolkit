using DevInstance.BlazorToolkit.Model;
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
    /// Puts (updates) an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="obj">The entity to update.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Put(string? id, T obj);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Delete(string? id);

    /// <summary>
    /// Limits the number of entities to retrieve.
    /// </summary>
    /// <param name="top">The number of entities to retrieve.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Top(int top);

    /// <summary>
    /// Sets the page number for pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Page(int page);

    /// <summary>
    /// Searches for entities by a key.
    /// </summary>
    /// <param name="key">The search key.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Search(string key);

    /// <summary>
    /// Sorts the entities by a key.
    /// </summary>
    /// <param name="key">The key to sort by.</param>
    /// <param name="isAsc">If true, sorts in ascending order; otherwise, sorts in descending order.</param>
    /// <returns>The API context.</returns>
    IApiContext<T> Sort(string key, bool isAsc);

    /// <summary>
    /// Executes the API context operation asynchronously.
    /// </summary>
    /// <returns>The entity.</returns>
    Task<T?> ExecuteAsync();

    /// <summary>
    /// Lists the entities asynchronously.
    /// </summary>
    /// <returns>A list of entities.</returns>
    Task<ModelList<T>?> ListAsync();
}

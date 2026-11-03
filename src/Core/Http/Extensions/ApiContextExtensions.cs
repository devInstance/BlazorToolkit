namespace DevInstance.BlazorToolkit.Http.Extensions;

public static class ApiContextExtensions
{
    /// <summary>
    /// Limits the number of entities to retrieve.
    /// </summary>
    /// <param name="top">The number of entities to retrieve.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<T> Top<T>(this IApiContext<T> context, int top)
    {
        context.Top<string, T>(top);
        return context;
    }
    /// <summary>
    /// Limits the number of entities to retrieve.
    /// </summary>
    /// <param name="top">The number of entities to retrieve.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<K, T> Top<K, T>(this IApiContext<K, T> context, int top)
    {
        context.Parameter("top", top);
        return context;
    }

    /// <summary>
    /// Sets the page number for pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<T> Page<T>(this IApiContext<T> context, int page)
    {
        context.Page<string, T>(page);
        return context;
    }

    /// <summary>
    /// Sets the page number for pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<K, T> Page<K, T>(this IApiContext<K, T> context, int page)
    {
        context.Parameter("page", page);
        return context;
    }

    /// <summary>
    /// Searches for entities by a key.
    /// </summary>
    /// <param name="key">The search key.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<T> Search<T>(this IApiContext<T> context, string key)
    {
        context.Search<string, T>(key);
        return context;
    }

    /// <summary>
    /// Searches for entities by a key.
    /// </summary>
    /// <param name="key">The search key.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<K, T> Search<K, T>(this IApiContext<K, T> context, string key)
    {
        context.Parameter("search", key);
        return context;
    }

    /// <summary>
    /// Sorts the entities by a key.
    /// </summary>
    /// <param name="key">The key to sort by.</param>
    /// <param name="isAsc">If true, sorts in ascending order; otherwise, sorts in descending order.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<T> Sort<T>(this IApiContext<T> context, string key, bool isAsc)
    {
        context.Sort<string, T>(key, isAsc);
        return context;
    }

    /// <summary>
    /// Sorts the entities by a key.
    /// </summary>
    /// <param name="key">The key to sort by.</param>
    /// <param name="isAsc">If true, sorts in ascending order; otherwise, sorts in descending order.</param>
    /// <returns>The API context.</returns>
    public static IApiContext<K, T> Sort<K, T>(this IApiContext<K, T> context, string key, bool isAsc)
    {   
        context.Parameter("sortBy", key);
        context.Parameter("asc", isAsc);
        return context;
    }
}

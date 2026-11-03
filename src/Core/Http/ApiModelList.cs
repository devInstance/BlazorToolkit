using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Http;

/// <summary>
/// Concrete <see cref="IModelList{T}"/> that <see cref="IApiContext{K, T}.ExecuteModelListAsync"/>
/// deserializes list responses into, so callers don't have to define their own list class.
/// </summary>
internal sealed class ApiModelList<T> : IModelList<T>
{
    public int TotalCount { get; set; }
    public int PagesCount { get; set; }
    public int Page { get; set; }
    public int Count { get; set; }
    public string[] SortOrder { get; set; }
    public string Search { get; set; }
    public T[] Items { get; set; }
}

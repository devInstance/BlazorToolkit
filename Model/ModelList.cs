namespace DevInstance.BlazorToolkit.Model;

public class ModelList<T>
{
    /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// Total count of pages
    /// </summary>
    public int PagesCount { get; set; }
    /// <summary>
    /// Selected page index (starting from 0)
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// Count of item on in selected time range
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// Column name to sort by
    /// </summary>
    public string SortBy { get; set; }
    /// <summary>
    /// If true - sort in ascending order
    /// </summary>
    public bool IsAsc { get; set; }
    /// <summary>
    /// Search string
    /// </summary>
    public string Search { get; set; }
    /// <summary>
    /// Filter value
    /// </summary>
    public int Filter { get; set; }
    /// <summary>
    /// Fields to include in response
    /// </summary>
    public int Fields { get; set; }

    /// <summary>
    /// Array of items
    /// </summary>
    public T[] Items { get; set; }
}

namespace DevInstance.BlazorToolkit.Model;


/// <summary>
/// Represents a list of models with pagination, sorting, and filtering capabilities.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public class ModelList<T>
{
    /// <summary>
    /// Gets or sets the total count of items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total count of pages.
    /// </summary>
    public int PagesCount { get; set; }

    /// <summary>
    /// Gets or sets the selected page index (starting from 0).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the count of items in the selected time range.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the column name to sort by.
    /// </summary>
    public string SortBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to sort in ascending order.
    /// </summary>
    public bool IsAsc { get; set; }

    /// <summary>
    /// Gets or sets the search string.
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// Gets or sets the filter value.
    /// </summary>
    public int Filter { get; set; }

    /// <summary>
    /// Gets or sets the fields to include in the response.
    /// </summary>
    public int Fields { get; set; }

    /// <summary>
    /// Gets or sets the array of items.
    /// </summary>
    public T[] Items { get; set; }
}

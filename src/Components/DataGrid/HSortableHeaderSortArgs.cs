namespace DevInstance.BlazorToolkit.Components.DataGrid;

public class HSortableHeaderSortArgs : EventArgs
{
    public string SortBy { get; set; } = string.Empty;
    public bool IsAscending { get; set; } = true;

    public HSortableHeaderSortArgs(string sortBy, bool isAscending)
    {
        SortBy = sortBy;
        IsAscending = isAscending;
    }
}

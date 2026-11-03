namespace DevInstance.BlazorToolkit.Components.DataGrid;

/// <summary>
/// One column's width after the user drags a header border in <c>HDataGrid</c> (sent by <c>blazortoolkit-components.js</c>).
/// </summary>
public class ColumnWidthChange
{
    public string Field { get; set; } = string.Empty;

    /// <summary>A percentage, e.g. <c>"23.50%"</c>.</summary>
    public string Width { get; set; } = string.Empty;
}

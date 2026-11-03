using Microsoft.AspNetCore.Components;

namespace DevInstance.BlazorToolkit.Components.DataGrid;

public class ColumnDescriptor<TItem>
{
    public string Field { get; init; } = default!;
    public string Label { get; init; } = default!;
    public Func<TItem, object?> ValueSelector { get; init; } = default!;

    /// <summary>
    /// The DTO property (<c>nameof(Item.Prop)</c>) whose search highlights apply to this column's value.
    /// When set, a cell without a template renders the value through <c>SearchHighlight</c>, using the
    /// ranges the server put in <c>IModelListHighlights.Highlights</c> (when the grid's <c>Data</c>
    /// implements it); a templated cell does it itself.
    /// </summary>
    public string? HighlightField { get; init; }

    public RenderFragment<object?>? Template { get; init; }
    public RenderFragment<TItem>? CellTemplate { get; init; }

    public bool IsVisible { get; set; } = true;

    public bool IsDragable { get; set; } = false;

    public bool IsSortable { get; set; } = true;

    private bool? _isGroupable;

    /// <summary>
    /// Whether the user may group the grid by this column (header menu, Grid Settings). Defaults to
    /// <see cref="IsSortable"/>: an unsortable column is usually an action or computed cell whose value
    /// makes a meaningless group key.
    /// </summary>
    public bool IsGroupable
    {
        get => _isGroupable ?? IsSortable;
        set => _isGroupable = value;
    }

    /// <summary>
    /// The column the grid is grouped by. At most one column is grouped; the grid keeps that true.
    /// Pages that save the user's layout persist it alongside the column order and widths.
    /// </summary>
    public bool IsGrouped { get; set; }

    /// <summary>
    /// The group key when grouping by this column, or null to group on <see cref="ValueSelector"/>.
    /// Set it when the displayed value is too fine-grained to group on (e.g. a timestamp → its date).
    /// </summary>
    public Func<TItem, object?>? GroupKeySelector { get; init; }

    public string Class { get; set; } = string.Empty;

    public string? Width { get; set; }

    /// <summary>
    /// The width the user dragged this column to (a percentage), or null for <see cref="Width"/>.
    /// Pages that save the user's layout persist it; it is kept apart from <see cref="Width"/> so
    /// "Reset Widths" can return to the page's defaults.
    /// </summary>
    public string? ResizedWidth { get; set; }
    public string? HeaderClass { get; set; }
    public string? CellClass { get; set; }
}

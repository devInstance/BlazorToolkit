using Microsoft.AspNetCore.Components;

namespace DevInstance.BlazorToolkit.Components.DataGrid;

public class GridSettingsResult<TItem>
{
    public List<ColumnDescriptor<TItem>> Columns { get; set; } = new();
    public int PageSize { get; set; }
}

internal static class GridSettingsDefaults
{
    public const string Id = "offcanvasGridSettings";
}

public partial class GridSettings<TItem>
{
    /// <summary>
    /// The drawer's element <c>id</c> (also used for its toggle button and title). Give each
    /// <see cref="GridSettings{TItem}"/> on a page its own, and pass the same value to
    /// <see cref="HDataGrid{TItem}.SettingsId"/> so the grid's header menu opens the right drawer.
    /// </summary>
    [Parameter]
    public string Id { get; set; } = GridSettingsDefaults.Id;

    /// <summary>
    /// Renders the fixed toggle button on the right edge of the viewport. Turn it off when the page opens
    /// the drawer some other way (the grid's header menu "Settings" item, or its own button with
    /// <c>data-bs-toggle="offcanvas" data-bs-target="#{Id}"</c>) — e.g. with two grids on one page, whose
    /// buttons would otherwise sit on top of each other.
    /// </summary>
    [Parameter]
    public bool ShowToggleButton { get; set; } = true;

    private List<ColumnDescriptor<TItem>> DefaultColumns { get; set; } = default!;
    private string? DefaultGroupedField { get; set; }

    [Parameter, EditorRequired]
    public List<ColumnDescriptor<TItem>> Columns { get; set; } = default!;

    [Parameter, EditorRequired]
    public EventCallback<GridSettingsResult<TItem>> OnSave { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>Shows the "Group by" picker. Turn it off where the grid's grouping is page-defined (HDataGrid.GroupBy).</summary>
    [Parameter]
    public bool EnableGrouping { get; set; } = true;

    private bool EnableResetButton = false;
    private ColumnDescriptor<TItem>? draggableEntry = null;

    protected override Task OnParametersSetAsync()
    {
        DefaultColumns = Columns.ToList();
        DefaultGroupedField = GroupedField;
        return Task.CompletedTask;
    }

    private async Task DoSave()
    {
        var result = new GridSettingsResult<TItem>
        {
            Columns = Columns,
            PageSize = PageSize
        };
        await OnSave.InvokeAsync(result);
    }

    private void DoReset()
    {
        Columns = DefaultColumns.ToList();
        SetGroupedField(DefaultGroupedField);
        EnableResetButton = false;
        StateHasChanged();
    }

    private IEnumerable<ColumnDescriptor<TItem>> GroupableColumns => Columns.Where(c => c.IsGroupable);

    private string GroupedField => Columns.FirstOrDefault(c => c.IsGrouped && c.IsGroupable)?.Field ?? "";

    private void OnGroupByChanged(ChangeEventArgs e)
    {
        SetGroupedField(e.Value?.ToString());
        EnableResetButton = true;
    }

    private void SetGroupedField(string? field)
    {
        foreach (var col in Columns)
            col.IsGrouped = !string.IsNullOrEmpty(field) && col.Field == field;
    }

    public void HandleMouseDown(ColumnDescriptor<TItem> entry)
    {
        entry.IsDragable = true;
    }

    public void HandleMouseUp(ColumnDescriptor<TItem> entry)
    {
        entry.IsDragable = false;
    }

    public void HandleDragEnter(ColumnDescriptor<TItem> entry)
    {
        if (draggableEntry?.Field == entry.Field) return;
        entry.Class = "column-can-drop";
    }

    public void HandleDragLeave(ColumnDescriptor<TItem> entry)
    {
        entry.Class = "";
    }

    public void HandleDrop(ColumnDescriptor<TItem> entry)
    {
        entry.Class = "";
        entry.IsDragable = false;

        if (draggableEntry != null && draggableEntry.Field == entry.Field) return;

        int newIndex = -1;
        int oldIndex = -1;
        for (int i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];
            if (col.Field == entry.Field)
            {
                newIndex = i;
            }
            else if (draggableEntry != null && col.Field == draggableEntry.Field)
            {
                oldIndex = i;
            }
        }

        if (newIndex >= 0 && oldIndex >= 0)
        {
            var ent = Columns[oldIndex];
            Columns.RemoveAt(oldIndex);
            Columns.Insert(newIndex, ent);
        }

        EnableResetButton = true;
        draggableEntry = null;
    }

    public void HandleDragStart(ColumnDescriptor<TItem> entry)
    {
        if (draggableEntry != null && draggableEntry.Field == entry.Field) return;

        draggableEntry = entry;
        entry.Class = "column-dragging";
    }
}

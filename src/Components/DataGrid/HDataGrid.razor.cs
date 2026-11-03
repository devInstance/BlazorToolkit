using DevInstance.BlazorToolkit.Components.Search;
using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace DevInstance.BlazorToolkit.Components.DataGrid;

public partial class HDataGrid<TItem> where TItem : IModelItem
{
    private static readonly int[] PlaceholderWidths = [75, 60, 85, 70, 90, 65, 80];

    private readonly Dictionary<object, bool> _expandedGroups = new();
    private readonly Dictionary<string, RenderFragment<TItem>> _columnTemplates = new();

    private const string ModulePath = "./_content/DevInstance.BlazorToolkit.Components/js/blazortoolkit-components.js";

    private readonly string _gridId = $"hdg-{Guid.NewGuid():N}";
    private DotNetObjectReference<HDataGrid<TItem>>? _selfRef;
    private Task<IJSObjectReference>? _moduleTask;

    private string? _groupedField;

    private ColumnDescriptor<TItem>? _contextMenuColumn;
    private double _contextMenuX;
    private double _contextMenuY;

    /// <summary>
    /// The page of items to show. When it also implements <see cref="IModelListHighlights"/>, columns with a
    /// <see cref="ColumnDescriptor{TItem}.HighlightField"/> render the server's search matches.
    /// </summary>
    [Parameter, EditorRequired]
    public IModelList<TItem>? Data { get; set; }

    [Parameter, EditorRequired]
    public List<ColumnDescriptor<TItem>> Columns { get; set; } = [];

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public int PlaceholderRows { get; set; } = 5;

    [Parameter]
    public EventCallback<HSortableHeaderSortArgs> OnSort { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    /// <summary>
    /// When false, the grid does not render its own pager (the host page
    /// renders a <see cref="ModelDataPager{T}"/> itself — e.g. outside a panel).
    /// Defaults to true to preserve existing behavior.
    /// </summary>
    [Parameter]
    public bool ShowPager { get; set; } = true;

    [Parameter]
    public bool ShowAllEnabled { get; set; }

    [Parameter]
    public bool ShowAll { get; set; }

    [Parameter]
    public EventCallback<bool> OnShowAllChanged { get; set; }

    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    [Parameter]
    public RenderFragment<string>? SearchContent { get; set; }

    [Parameter]
    public bool EnableSelection { get; set; }

    [Parameter]
    public HashSet<string>? SelectedIds { get; set; }

    [Parameter]
    public EventCallback<HashSet<string>> SelectedIdsChanged { get; set; }

    [Parameter]
    public Func<TItem, string>? IdSelector { get; set; }

    [Parameter]
    public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>
    /// A page-defined grouping. When set it wins over column grouping, and the grid offers no
    /// grouping of its own (see <see cref="EnableGrouping"/>).
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? GroupBy { get; set; }

    /// <summary>
    /// Lets the user group by a column — from the header menu, the group bar or Grid Settings. The choice
    /// lives in <see cref="ColumnDescriptor{TItem}.IsGrouped"/> and is announced through <see cref="OnColumnsChanged"/>.
    /// </summary>
    [Parameter]
    public bool EnableGrouping { get; set; } = true;

    [Parameter]
    public RenderFragment<GroupContext<TItem>>? GroupHeaderTemplate { get; set; }

    [Parameter]
    public bool GroupsCollapsedByDefault { get; set; }

    [Parameter]
    public string? TableClass { get; set; }

    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    [Parameter]
    public RenderFragment? BeforeHeaderRow { get; set; }

    [Parameter]
    public RenderFragment? ColumnTemplates { get; set; }

    [Parameter]
    public EventCallback OnColumnsChanged { get; set; }

    /// <summary>
    /// Lets the user drag header borders to resize columns. The new widths land in
    /// <see cref="ColumnDescriptor{TItem}.ResizedWidth"/> and are announced through <see cref="OnColumnsChanged"/>.
    /// </summary>
    [Parameter]
    public bool ResizableColumns { get; set; } = true;

    /// <summary>
    /// The <c>id</c> of the <see cref="GridSettings{TItem}"/> drawer the header menu's "Settings" item opens.
    /// Must match that component's <see cref="GridSettings{TItem}.Id"/>; give each grid on a page its own.
    /// </summary>
    [Parameter]
    public string SettingsId { get; set; } = GridSettingsDefaults.Id;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override void OnParametersSet()
    {
        SyncGroupedField();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && ResizableColumns)
        {
            _selfRef = DotNetObjectReference.Create(this);
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("registerGrid", _gridId, _selfRef);
        }
    }

    // Imported on first use (after the first render — never during prerendering), once per grid instance;
    // the browser caches the module itself, so every grid on the page shares one copy of the script.
    private Task<IJSObjectReference> GetModuleAsync() =>
        _moduleTask ??= JS.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask != null)
            {
                var module = await _moduleTask;
                if (_selfRef != null)
                    await module.InvokeVoidAsync("unregisterGrid", _gridId);
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Blazor Server: the circuit is gone, and the browser side with it.
        }
        _selfRef?.Dispose();
    }

    [JSInvokable]
    public async Task OnColumnsResized(ColumnWidthChange[] widths)
    {
        foreach (var change in widths)
        {
            var col = Columns.FirstOrDefault(c => c.Field == change.Field);
            if (col != null) col.ResizedWidth = change.Width;
        }
        StateHasChanged();
        await OnColumnsChanged.InvokeAsync();
    }

    // Called from HColumnTemplate.OnInitialized, which runs after this grid has already rendered its rows
    // without the template. Re-render once so the cells pick it up — otherwise, with data that is
    // already there on the first render (no later async load to trigger a re-render), they never do.
    internal void RegisterColumnTemplate(string field, RenderFragment<TItem> template)
    {
        if (_columnTemplates.TryGetValue(field, out var existing) && existing == template)
            return;
        _columnTemplates[field] = template;
        StateHasChanged();
    }

    /// <summary>The server's search matches for one field of a row (see <see cref="ColumnDescriptor{TItem}.HighlightField"/>).</summary>
    private HighlightRange[]? HighlightsFor(TItem item, string field) =>
        Data.HighlightsFor(IdSelector?.Invoke(item) ?? item.Id, field);

    private RenderFragment<TItem>? GetCellTemplate(ColumnDescriptor<TItem> col)
    {
        if (_columnTemplates.TryGetValue(col.Field, out var template))
            return template;
        return col.CellTemplate;
    }

    private IEnumerable<ColumnDescriptor<TItem>> VisibleColumns => Columns.Where(c => c.IsVisible);

    // Includes the cell padding (border-box): a 1rem checkbox plus padding. Narrower and the checkbox overflows
    // the cell, which the CRM table styles render as an ellipsis.
    private const string SelectionColumnWidth = "2.75rem";

    private string HeaderWidthStyle(ColumnDescriptor<TItem> col) =>
        GridLayout.HeaderWidthStyle(EffectiveWidth(col), VisibleColumns.Select(EffectiveWidth));

    private static string? EffectiveWidth(ColumnDescriptor<TItem> col) => col.ResizedWidth ?? col.Width;

    private int TotalColumnCount => VisibleColumns.Count() + (EnableSelection ? 1 : 0);

    private bool IsAllSelected =>
        Data?.Items != null
        && Data.Items.Any()
        && SelectedIds != null
        && IdSelector != null
        && Data.Items.All(item => SelectedIds.Contains(IdSelector(item)));

    private void OnSelectAllChanged(ChangeEventArgs e)
    {
        if (Data?.Items == null || SelectedIds == null || IdSelector == null) return;

        var selectAll = (bool)(e.Value ?? false);
        if (selectAll)
        {
            foreach (var item in Data.Items)
                SelectedIds.Add(IdSelector(item));
        }
        else
        {
            foreach (var item in Data.Items)
                SelectedIds.Remove(IdSelector(item));
        }

        SelectedIdsChanged.InvokeAsync(SelectedIds);
    }

    private void OnRowSelectChanged(TItem item, bool isSelected)
    {
        if (SelectedIds == null || IdSelector == null) return;

        var id = IdSelector(item);
        if (isSelected)
            SelectedIds.Add(id);
        else
            SelectedIds.Remove(id);

        SelectedIdsChanged.InvokeAsync(SelectedIds);
    }

    // Grouping

    private bool ColumnGroupingEnabled => EnableGrouping && GroupBy == null;

    /// <summary>The column the user grouped by, when column grouping is in effect.</summary>
    private ColumnDescriptor<TItem>? GroupedColumn =>
        ColumnGroupingEnabled ? Columns.FirstOrDefault(c => c.IsGrouped && c.IsGroupable) : null;

    private bool IsGrouping => GroupBy != null || GroupedColumn != null;

    private Func<TItem, object?>? GroupKeySelector =>
        GroupBy ?? (GroupedColumn is { } col ? col.GroupKeySelector ?? col.ValueSelector : null);

    // Groups are built from the current page only. A page-defined grouping keeps the order the rows came in;
    // a column grouping orders its groups by key — unless the data is sorted on that column, where the server's
    // order (and direction) already is the key order.
    private List<GroupContext<TItem>> GetGroups()
    {
        var selector = GroupKeySelector;
        if (Data?.Items == null || selector == null) return [];

        var groups = Data.Items
            .GroupBy(selector)
            .Select(g => new GroupContext<TItem>
            {
                Key = g.Key,
                Items = g.ToList(),
                Count = g.Count()
            });

        var col = GroupedColumn;
        if (col != null && !string.Equals(GridLayout.SortField(Data.SortOrder), col.Field, StringComparison.OrdinalIgnoreCase))
            groups = groups.OrderBy(g => g.Key, GridLayout.GroupKeyComparer.Instance);

        return groups.ToList();
    }

    // A different grouping has different keys; drop the expand/collapse state of the old ones.
    private void SyncGroupedField()
    {
        var field = GroupedColumn?.Field;
        if (field == _groupedField) return;
        _groupedField = field;
        _expandedGroups.Clear();
    }

    private async Task SetGroupedColumn(ColumnDescriptor<TItem>? column)
    {
        foreach (var col in Columns)
            col.IsGrouped = col == column;
        SyncGroupedField();
        await OnColumnsChanged.InvokeAsync();
    }

    private void SetAllGroupsExpanded(bool expanded)
    {
        foreach (var group in GetGroups())
            _expandedGroups[group.Key ?? "__null__"] = expanded;
    }

    private bool IsGroupExpanded(object? key)
    {
        if (_expandedGroups.TryGetValue(key ?? "__null__", out var expanded))
            return expanded;

        return !GroupsCollapsedByDefault;
    }

    private void ToggleGroup(object? key)
    {
        var normalizedKey = key ?? "__null__";
        _expandedGroups[normalizedKey] = !IsGroupExpanded(key);
    }

    private async Task ToggleShowAll()
    {
        await OnShowAllChanged.InvokeAsync(!ShowAll);
    }

    // Context menu

    private bool IsContextMenuVisible => _contextMenuColumn != null;

    private bool IsContextMenuColumnSortable => _contextMenuColumn?.IsSortable == true;

    private bool IsContextMenuColumnGroupable => ColumnGroupingEnabled && _contextMenuColumn?.IsGroupable == true;

    private bool IsContextMenuColumnGrouped => _contextMenuColumn != null && _contextMenuColumn == GroupedColumn;

    private bool IsFirstVisibleColumn =>
        _contextMenuColumn != null && Columns.IndexOf(_contextMenuColumn) == 0;

    private bool IsLastVisibleColumn =>
        _contextMenuColumn != null && Columns.IndexOf(_contextMenuColumn) == Columns.Count - 1;

    private void OnHeaderContextMenu(MouseEventArgs e, ColumnDescriptor<TItem> col)
    {
        _contextMenuColumn = col;
        _contextMenuX = e.ClientX;
        _contextMenuY = e.ClientY;
    }

    private void CloseContextMenu()
    {
        _contextMenuColumn = null;
    }

    private async Task ContextMenuSortAsc()
    {
        if (_contextMenuColumn is { IsSortable: true })
        {
            await OnSort.InvokeAsync(new HSortableHeaderSortArgs(_contextMenuColumn.Field, true));
        }
        CloseContextMenu();
    }

    private async Task ContextMenuSortDesc()
    {
        if (_contextMenuColumn is { IsSortable: true })
        {
            await OnSort.InvokeAsync(new HSortableHeaderSortArgs(_contextMenuColumn.Field, false));
        }
        CloseContextMenu();
    }

    private async Task ContextMenuMoveLeft()
    {
        if (_contextMenuColumn != null)
        {
            var idx = Columns.IndexOf(_contextMenuColumn);
            if (idx > 0)
            {
                Columns.RemoveAt(idx);
                Columns.Insert(idx - 1, _contextMenuColumn);
                await OnColumnsChanged.InvokeAsync();
            }
        }
        CloseContextMenu();
    }

    private async Task ContextMenuMoveRight()
    {
        if (_contextMenuColumn != null)
        {
            var idx = Columns.IndexOf(_contextMenuColumn);
            if (idx >= 0 && idx < Columns.Count - 1)
            {
                Columns.RemoveAt(idx);
                Columns.Insert(idx + 1, _contextMenuColumn);
                await OnColumnsChanged.InvokeAsync();
            }
        }
        CloseContextMenu();
    }

    private bool HasResizedColumns => Columns.Any(c => c.ResizedWidth != null);

    private async Task ContextMenuResetWidths()
    {
        foreach (var col in Columns)
            col.ResizedWidth = null;
        CloseContextMenu();
        await OnColumnsChanged.InvokeAsync();
    }

    private async Task ContextMenuToggleGroup()
    {
        var column = _contextMenuColumn;
        CloseContextMenu();
        if (column != null)
            await SetGroupedColumn(IsGroupedColumn(column) ? null : column);
    }

    private bool IsGroupedColumn(ColumnDescriptor<TItem> column) => column == GroupedColumn;

    private async Task ContextMenuHideColumn()
    {
        if (_contextMenuColumn != null)
        {
            _contextMenuColumn.IsVisible = false;
            await OnColumnsChanged.InvokeAsync();
        }
        CloseContextMenu();
    }

    private async Task ContextMenuShowSettings()
    {
        CloseContextMenu();
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("showOffcanvas", SettingsId);
        }
        catch (JSDisconnectedException)
        {
        }
    }
}

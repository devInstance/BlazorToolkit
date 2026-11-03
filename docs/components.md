# HDataGrid and search components

`DevInstance.BlazorToolkit.Components` (10.7.0+) ships a data grid and a search bar:

| Component | Namespace | Purpose |
|-----------|-----------|---------|
| `HDataGrid<TItem>` | `DevInstance.BlazorToolkit.Components.DataGrid` | Grid over an `IModelList<TItem>`: sorting, paging, row selection, grouping, column resizing, column header context menu, search highlights |
| `HColumnTemplate<TItem>` | `….DataGrid` | Razor cell template for one column |
| `GridSettings<TItem>` | `….DataGrid` | Offcanvas drawer: column order and visibility, group-by, page size |
| `ColumnDescriptor<TItem>` | `….DataGrid` | Column definition |
| `HSortableHeader` / `HSortableHeaderSortArgs` | `….DataGrid` | The sortable header cell the grid uses, and its sort event args |
| `HSearchBar` | `DevInstance.BlazorToolkit.Components.Search` | Search input with an optional "advanced" dropdown |
| `SearchHighlight` | `….Search` | Renders text with server-provided highlight ranges as `<mark>` |

The sample app's `/grid` page (`example/DevInstance.BlazorToolkit.Samples.Client/Pages/Grid.razor`) shows all of it, including two grids on one page.

## Prerequisites

1. **Packages:** `DevInstance.BlazorToolkit.Components` 10.7.0+. It brings in `DevInstance.BlazorToolkit` and `DevInstance.WebServiceToolkit.Common` 10.4.0+ (for `IModelList<T>`, `IModelListHighlights`, `HighlightRange`).
2. **Bootstrap 5.3 CSS and JS bundle, and Bootstrap Icons.** The JS bundle is required: the settings drawer is a Bootstrap offcanvas and the header context menu uses Bootstrap's styles. Load the bundle before the Blazor script.
3. **The package stylesheet**, after Bootstrap and before your own CSS:

   ```html
   <link rel="stylesheet" href="_content/DevInstance.BlazorToolkit.Components/css/blazortoolkit-components.css" />
   ```

   In a Blazor Web App `App.razor` use `@Assets["_content/DevInstance.BlazorToolkit.Components/css/blazortoolkit-components.css"]`.
4. **No `<script>` tag for the grid.** `HDataGrid` imports its ES module (`_content/DevInstance.BlazorToolkit.Components/js/blazortoolkit-components.js`) itself after its first render.
5. **`@using` lines**, e.g. in `_Imports.razor`:

   ```razor
   @using DevInstance.BlazorToolkit.Components.DataGrid
   @using DevInstance.BlazorToolkit.Components.Search
   ```

6. **Interactive render mode.** The grid needs interactivity (Server or WebAssembly) for sorting, menus and resizing.

## Quick start

### 1. Define columns

```csharp
private List<ColumnDescriptor<MyItem>> Columns { get; set; } = new()
{
    new() { Label = "Name",   Field = "name",   ValueSelector = x => x.Name },
    new() { Label = "Email",  Field = "email",  ValueSelector = x => x.Email },
    new() { Label = "Status", Field = "status", ValueSelector = x => x.Status.ToString() },
    new() { Label = "Notes",  Field = "notes",  ValueSelector = x => x.Notes, IsSortable = false, IsVisible = false },
};
```

`Field` is the key used for sorting (it is what `OnSort` reports), template matching and persisted settings. Make it match your API's sort field.

### 2. Place the grid

```razor
<HDataGrid TItem="MyItem"
           Data="ItemList"
           Columns="Columns"
           IsLoading="InProgress"
           OnSort="OnSortAsync"
           OnPageChanged="OnPageChangedAsync"
           OnColumnsChanged="OnColumnsChanged">
    <EmptyContent>
        <p>No items found.</p>
    </EmptyContent>
</HDataGrid>
```

`Data` is any `IModelList<MyItem>`: the list your service returns, or `IApiContext<T>.ExecuteModelListAsync()`. The grid reads `Items`, `Page`, `PagesCount`, `SortOrder` and `Search` from it. Sorting and paging are done by your code (normally the server): the grid only raises `OnSort` / `OnPageChanged`.

### 3. Add the settings drawer

```razor
<GridSettings TItem="MyItem" Columns="Columns" PageSize="pageSize" OnSave="OnSave" />
```

It is opened by the "Settings" item of the header context menu, and by its own fixed button on the right edge of the viewport (hide that with `ShowToggleButton="false"`).

### Two grids on one page

Each `GridSettings` needs its own element id, and the grid must know it so its "Settings" menu item opens the right drawer:

```razor
<HDataGrid TItem="Employee" ... SettingsId="employeeSettings" />
<HDataGrid TItem="Order"    ... SettingsId="orderSettings" />

<GridSettings TItem="Employee" Id="employeeSettings" ShowToggleButton="false" ... />
<GridSettings TItem="Order"    Id="orderSettings"    ShowToggleButton="false" ... />

@* Your own buttons, since the two fixed toggle buttons would overlap: *@
<button class="btn btn-outline-secondary" data-bs-toggle="offcanvas" data-bs-target="#employeeSettings">Columns</button>
```

Both default to `offcanvasGridSettings`, so a page with a single grid needs neither.

## HDataGrid parameters

### Data

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TItem` | type param | | Item type; must implement `IModelItem` |
| `Data` | `IModelList<TItem>?` | | The current page of data. `null` shows nothing |
| `Columns` | `List<ColumnDescriptor<TItem>>` | `[]` | Column definitions |
| `IsLoading` | `bool` | `false` | Shows skeleton rows |
| `PlaceholderRows` | `int` | `5` | Number of skeleton rows |
| `EmptyContent` | `RenderFragment?` | | Shown when `Data` has no items |
| `SearchContent` | `RenderFragment<string>?` | | Shown above the table while `Data.Search` is not empty; receives the search text |

### Sorting and paging

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `OnSort` | `EventCallback<HSortableHeaderSortArgs>` | | Header click or context menu sort. `SortBy` is the column's `Field` |
| `OnPageChanged` | `EventCallback<int>` | | Pager navigation (zero-based page) |
| `ShowPager` | `bool` | `true` | Show the pager below the table |
| `ShowAllEnabled` | `bool` | `false` | Show a "Show All / Show Paged" toggle |
| `ShowAll` | `bool` | `false` | Current show-all state |
| `OnShowAllChanged` | `EventCallback<bool>` | | The toggle was clicked |

The sorted column and direction come from `Data.SortOrder` (first entry; a leading `-` means descending).

### Selection

| Parameter | Type | Description |
|-----------|------|-------------|
| `EnableSelection` | `bool` | Adds a checkbox column with select-all |
| `SelectedIds` / `SelectedIdsChanged` | `HashSet<string>?` | Selected ids; supports `@bind-SelectedIds` |
| `IdSelector` | `Func<TItem, string>?` | Item id. **Required for selection** (checkboxes do nothing without it); also used to look up highlights (default `IModelItem.Id`) |

### Rows and table

| Parameter | Type | Description |
|-----------|------|-------------|
| `OnRowClick` | `EventCallback<TItem>` | Row click (adds a pointer cursor) |
| `RowClass` | `Func<TItem, string>?` | CSS class per row |
| `TableClass` | `string?` | Extra CSS class on `<table>` |
| `BeforeHeaderRow` | `RenderFragment?` | Extra rows before the header row |
| `ColumnTemplates` | `RenderFragment?` | Holds `<HColumnTemplate>` declarations |

### Grouping

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `EnableGrouping` | `bool` | `true` | The user may group by a column (see [Grouping](#grouping)) |
| `GroupBy` | `Func<TItem, object?>?` | | Page-defined group key; overrides and hides column grouping |
| `GroupHeaderTemplate` | `RenderFragment<GroupContext<TItem>>?` | | Custom group header (both kinds of grouping) |
| `GroupsCollapsedByDefault` | `bool` | `false` | Start groups collapsed |

### Columns and settings

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `OnColumnsChanged` | `EventCallback` | | After move / hide / group / reset widths from the context menu or group bar, and after a resize. Persist the columns here |
| `ResizableColumns` | `bool` | `true` | Drag a header's right edge to resize |
| `SettingsId` | `string` | `"offcanvasGridSettings"` | Element id of the `GridSettings` drawer the "Settings" menu item opens; must equal that drawer's `Id` |

## GridSettings parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Columns` | `List<ColumnDescriptor<TItem>>` | required | The same list the grid uses |
| `OnSave` | `EventCallback<GridSettingsResult<TItem>>` | required | Save clicked: `Columns` (new order/visibility/grouping) and `PageSize` |
| `PageSize` | `int` | `10` | Initial page size in the picker |
| `EnableGrouping` | `bool` | `true` | Show the "Group by" picker. Turn off when the grid uses `GroupBy` |
| `Id` | `string` | `"offcanvasGridSettings"` | Element id of the drawer |
| `ShowToggleButton` | `bool` | `true` | Render the fixed toggle button on the viewport's right edge |

## ColumnDescriptor

```csharp
public class ColumnDescriptor<TItem>
{
    string   Field          // Unique key: sort field, template matching, persisted settings
    string   Label          // Header text
    Func<TItem, object?> ValueSelector  // Cell value
    string?  HighlightField // Property name whose search highlights apply (see "Search highlighting")

    RenderFragment<object?>? Template      // Value template (receives ValueSelector's result)
    RenderFragment<TItem>?   CellTemplate  // Item template (receives the item)

    bool     IsVisible      // true
    bool     IsSortable     // true; enables the sort header and the menu's sort items
    bool     IsGroupable    // defaults to IsSortable; offered in "Group by"
    bool     IsGrouped      // the column the grid is grouped by (at most one)
    Func<TItem, object?>? GroupKeySelector  // group key override, e.g. a date's year; default ValueSelector

    string?  Width          // Default width ("200px", "30%")
    string?  ResizedWidth   // Width the user dragged to (a percentage); overrides Width
    string?  HeaderClass    // CSS class on <th>
    string?  CellClass      // CSS class on <td>

    bool     IsDragable     // internal: GridSettings drag-and-drop
    string   Class          // internal: GridSettings drag state
}
```

To persist a user's layout, store per column `Field`, `IsVisible`, `IsGrouped`, `ResizedWidth` and the list order (plus page size and sort), and re-apply them to your column list on load. The package has no storage of its own.

## Cell templates

Checked in this order:

1. **`<HColumnTemplate>`** (preferred for markup). `Field` must match a column's `Field`.

   ```razor
   <HDataGrid TItem="MyItem" ...>
       <ColumnTemplates>
           <HColumnTemplate TItem="MyItem" Field="actions" Context="item">
               <a href="items/@item.Id/edit" class="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i></a>
               <button class="btn btn-sm btn-outline-danger" @onclick="() => Delete(item)"><i class="bi bi-trash"></i></button>
           </HColumnTemplate>
       </ColumnTemplates>
   </HDataGrid>
   ```

2. **`CellTemplate`** on the descriptor: receives the item. For templates built in code.
3. **`Template`** on the descriptor: formats the `ValueSelector` result.

   ```csharp
   new() { Label = "Status", Field = "status", ValueSelector = e => e.Status,
           Template = value => @<span class="badge bg-info">@value</span> }
   ```

4. **Default:** the value as text (with search highlights when `HighlightField` is set).

## Search highlighting

The server marks where a search matched; the grid only draws it. Item fields stay plain values: never put `<mark>` into a DTO.

1. Make your list type implement `IModelListHighlights` alongside `IModelList<T>` (both from `DevInstance.WebServiceToolkit.Common.Model`):

   ```csharp
   public class EmployeeList : IModelList<Employee>, IModelListHighlights
   {
       // ... IModelList<Employee> members ...
       public Dictionary<string, Dictionary<string, HighlightRange[]>>? Highlights { get; set; }
   }
   ```

   `Highlights` maps item id → property name → match ranges (`Start`, `Length`). The server fills it next to the query's search, so the highlight always agrees with what was found.
2. Set `HighlightField = nameof(Employee.Name)` on the column. The grid renders that cell through `<SearchHighlight>` with the field's ranges. It checks `Data is IModelListHighlights`; a list without highlights renders plain text.
3. Templated cells do it themselves with the `HighlightsFor` extension on `IModelList<T>` (namespace `DevInstance.BlazorToolkit.Components.Search`):

   ```razor
   <SearchHighlight Text="@item.Name" Ranges="@List.HighlightsFor(item.Id, nameof(MyItem.Name))" />
   ```

`SearchHighlight` writes text nodes and `<mark class="search-mark">`, so it is safe for any content. It clips and merges ranges, so a truncated preview can pass the whole field's ranges. `HighlightRange.Segments(text, ranges)` exposes the same splitting for your own rendering.

### HSearchBar

```razor
<HSearchBar @bind-Value="search" OnSearch="SearchAsync" Placeholder="Search name or city..." />
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `Value` / `ValueChanged` | `string` | The search text; supports `@bind-Value` |
| `OnSearch` | `EventCallback` | Enter, the search button, or the dropdown's Search button |
| `Placeholder` | `string` | Default `"Search..."` |
| `Disabled` | `bool` | Disables the input |
| `AdvancedContent` | `RenderFragment?` | Adds a filter button that opens a dropdown with this content and a Search button |

## Column header context menu

Right-click a column header:

| Item | Action | Shown / enabled |
|------|--------|-----------------|
| Sort Asc / Sort Desc | Raises `OnSort` | Sortable columns |
| Move Left / Move Right | Swaps with the neighbour | Not at the edge |
| Group by This Column / Ungroup | Sets / clears `IsGrouped` | Groupable column, column grouping enabled |
| Reset Widths | Clears every `ResizedWidth` | After a resize |
| Hide | `IsVisible = false` | Always |
| Settings | Opens the drawer with id `SettingsId` | Always |

Move, group, hide and reset widths raise `OnColumnsChanged`.

## Column resizing

Every data column but the last has a grab strip on its header's right edge. Dragging it moves the border between that column and its right-hand neighbour, so the table's width stays the same; a column never goes below 48px. On release every visible column's width is stored in `ResizedWidth` as a percentage and `OnColumnsChanged` fires. `ResizableColumns="false"` turns it off.

When every visible column has a percentage width, the grid scales them to add up to 100%, so leftover space goes to them rather than to the selection column.

## Grouping

**Column grouping (default).** The user picks the column (header menu, or the "Group by" picker in `GridSettings`); the choice is stored on the column (`IsGrouped`) so it persists with the rest of the column settings. While grouped, a "Grouped by" bar above the table offers remove, expand all and collapse all.

- Groups are built from **the current page only**; paging is unchanged.
- Groups are ordered by key (nulls last), unless the data is sorted on the grouped column, in which case the data's order and direction are kept.
- The default group header is `Label: <key> <count>`. The key is drawn the way the column draws its cells: its item template for the group's first row, else its value `Template`, else text. A column with a `GroupKeySelector` skips the item template (the first row's value is not the key).
- A column is groupable when `IsGroupable` (default: `IsSortable`), so action columns are left out.

**Page-defined grouping.** Pass `GroupBy` when the page owns the grouping. It overrides column grouping and hides the grid's grouping UI; set `EnableGrouping="false"` on the page's `GridSettings` too.

```razor
<HDataGrid TItem="DealItem" ...
           GroupBy="@(groupByStage ? (d => d.StageName ?? "No Stage") : null)">
    <GroupHeaderTemplate>
        <strong>@context.Key</strong>
        <span class="badge bg-secondary ms-2">@context.Count</span>
    </GroupHeaderTemplate>
</HDataGrid>
```

`GroupContext<TItem>` has `Key`, `Items` and `Count`.

## Theming

Colours come from CSS custom properties that fall back to Bootstrap's variables, so the grid follows Bootstrap's light/dark mode out of the box. Override them on any ancestor (or `:root`) to match your design:

| Variable | Fallback | Used for |
|----------|----------|----------|
| `--btk-accent` | `--bs-primary` | Resize handle hover, `search-mark` text, settings drop target |
| `--btk-accent-bg` | `--bs-primary-bg-subtle` | `mark.search-mark` background |
| `--btk-border` | `--bs-border-color` | Borders, group bar |
| `--btk-surface` | `--bs-tertiary-bg` | Group bar background |
| `--btk-chip` | `--bs-secondary-bg` | Chips and counts |
| `--btk-bg` | `--bs-body-bg` | Backgrounds |
| `--btk-text` | `--bs-body-color` | Text |
| `--btk-text-2` | `--bs-secondary-color` | Secondary text |
| `--btk-text-3` | `--bs-tertiary-color` | Muted text (eyebrow labels) |

```css
.my-app {
    --btk-accent: #7c3aed;
    --btk-accent-bg: #ede9fe;
}
```

Grid classes are prefixed `hdg-` (`hdg-th`, `hdg-resizer`, `hdg-groupbar`, `hdg-chip`, `hdg-count`, `hdg-eyebrow`) and settings drawer classes `gs-`.

## Guidance for agents writing pages

1. Use `HDataGrid` for list pages instead of hand-written `<table>` markup.
2. Define columns as `List<ColumnDescriptor<TItem>>`; set `Field` to the API's sort field and `IsSortable = false` for computed columns.
3. Prefer `<HColumnTemplate>` in Razor over `CellTemplate` built in code.
4. Place a `<GridSettings>` next to the grid; with more than one grid on a page, give each its own `Id`/`SettingsId`.
5. Persist column settings from `OnColumnsChanged`, `OnSort` and `GridSettings.OnSave`.
6. Do not change grid internals for page-specific needs; use parameters and templates.

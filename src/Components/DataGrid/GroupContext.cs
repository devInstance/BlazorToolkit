namespace DevInstance.BlazorToolkit.Components.DataGrid;

public class GroupContext<TItem>
{
    public object? Key { get; init; }
    public IReadOnlyList<TItem> Items { get; init; } = [];
    public int Count { get; init; }
}

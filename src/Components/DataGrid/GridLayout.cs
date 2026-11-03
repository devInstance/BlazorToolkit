using System.Globalization;

namespace DevInstance.BlazorToolkit.Components.DataGrid;

/// <summary>
/// The pure layout and ordering logic behind <see cref="HDataGrid{TItem}"/>, kept apart so it can be unit-tested.
/// </summary>
internal static class GridLayout
{
    /// <summary>
    /// The <c>style</c> of a data column's header, given its own width and the widths of every visible
    /// data column (its own included).
    /// </summary>
    /// <remarks>
    /// With <c>table-layout: fixed</c>, slack left when the percentage widths add up to less than 100% (e.g.
    /// after hiding columns) goes to the pixel-width selection column, which then swells to hundreds of pixels.
    /// When every visible column has a percentage width, scale them to add up to 100% so they take the slack;
    /// the browser shrinks them proportionally to make room for the selection column. (A <c>calc()</c> mixing
    /// % and px would be exact, but Chrome treats it as auto in a table column.)
    /// </remarks>
    public static string HeaderWidthStyle(string? width, IEnumerable<string?> visibleWidths)
    {
        if (width == null)
            return "";

        var own = ParsePercent(width);
        var percents = visibleWidths.Select(ParsePercent).ToList();
        if (own == null || percents.Any(p => p == null))
            return $"width: {width};";

        var scaled = own.Value * 100 / percents.Sum(p => p!.Value);
        return $"width: {scaled.ToString("0.####", CultureInfo.InvariantCulture)}%;";
    }

    private static decimal? ParsePercent(string? width)
    {
        if (width == null || !width.EndsWith('%'))
            return null;
        return decimal.TryParse(width.AsSpan(0, width.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : null;
    }

    /// <summary>The primary sort field without its "+"/"-" direction prefix, or null when unsorted.</summary>
    public static string? SortField(string[]? sortOrder)
    {
        var first = sortOrder?.Length > 0 ? sortOrder[0] : null;
        return string.IsNullOrEmpty(first) ? null : first.TrimStart('+', '-');
    }

    /// <summary>Orders group keys: nulls last, comparable keys of one type by value, anything else by text.</summary>
    public sealed class GroupKeyComparer : IComparer<object?>
    {
        public static readonly GroupKeyComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (x == null || y == null)
                return x == null ? (y == null ? 0 : 1) : -1;
            if (x.GetType() == y.GetType() && x is IComparable comparable)
                return comparable.CompareTo(y);
            return string.Compare(x.ToString(), y.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}

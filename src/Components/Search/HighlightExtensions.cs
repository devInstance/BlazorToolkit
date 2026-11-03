using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Components.Search;

/// <summary>
/// Lookups into the server's search highlights of a list response (see <see cref="IModelListHighlights"/>).
/// </summary>
public static class HighlightExtensions
{
    /// <summary>
    /// The highlight ranges for one field of one item of <paramref name="list"/>, or null when the list
    /// carries no highlights (does not implement <see cref="IModelListHighlights"/>) or the field has none.
    /// <paramref name="field"/> is the DTO property name — pass <c>nameof(ContactItem.FullName)</c>.
    /// </summary>
    public static HighlightRange[]? HighlightsFor<T>(this IModelList<T>? list, string? itemId, string? field)
    {
        var highlights = (list as IModelListHighlights)?.Highlights;
        if (highlights == null || itemId == null || field == null)
            return null;
        return highlights.TryGetValue(itemId, out var fields) && fields.TryGetValue(field, out var ranges)
            ? ranges
            : null;
    }
}

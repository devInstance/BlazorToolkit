using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace DevInstance.BlazorToolkit.Components.Search;

/// <summary>
/// Renders <see cref="Text"/> with the server's search matches wrapped in <c>&lt;mark class="search-mark"&gt;</c>.
/// The ranges come from <see cref="IModelListHighlights.Highlights"/> — get them with
/// <c>list.HighlightsFor(item.Id, nameof(Item.Field))</c> (<see cref="HighlightExtensions"/>). Text is emitted as text nodes, never as markup,
/// so it is safe for any user or email content. Ranges past the end of <see cref="Text"/> are clipped, so a
/// truncated preview can pass the full field's ranges.
/// </summary>
/// <remarks>
/// A plain class rather than a <c>.razor</c> file on purpose: Razor markup would put whitespace text nodes
/// between the segments, which shows inside <c>white-space: pre-wrap</c> containers.
/// </remarks>
public class SearchHighlight : ComponentBase
{
    [Parameter] public string? Text { get; set; }

    [Parameter] public IEnumerable<HighlightRange>? Ranges { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        foreach (var (segment, isMatch) in HighlightRange.Segments(Text, Ranges))
        {
            if (isMatch)
            {
                builder.OpenElement(seq++, "mark");
                builder.AddAttribute(seq++, "class", "search-mark");
                builder.AddContent(seq++, segment);
                builder.CloseElement();
            }
            else
            {
                builder.AddContent(seq++, segment);
            }
        }
    }
}

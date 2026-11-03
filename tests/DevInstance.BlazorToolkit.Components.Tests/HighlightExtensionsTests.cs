using DevInstance.BlazorToolkit.Components.Search;
using DevInstance.WebServiceToolkit.Common.Model;
using Xunit;

namespace DevInstance.BlazorToolkit.Components.Tests;

public class HighlightExtensionsTests
{
    private class PlainList : IModelList<string>
    {
        public int TotalCount { get; set; }
        public int PagesCount { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
        public string[] SortOrder { get; set; } = [];
        public string Search { get; set; } = "";
        public string[] Items { get; set; } = [];
    }

    private class HighlightedList : PlainList, IModelListHighlights
    {
        public Dictionary<string, Dictionary<string, HighlightRange[]>>? Highlights { get; set; }
    }

    private static readonly HighlightedList List = new()
    {
        Highlights = new() { ["c1"] = new() { ["FullName"] = [new HighlightRange(0, 2)] } },
    };

    [Fact]
    public void finds_the_ranges_of_an_item_field()
    {
        var range = Assert.Single(List.HighlightsFor("c1", "FullName")!);
        Assert.Equal((0, 2), (range.Start, range.Length));
    }

    [Fact]
    public void missing_item_field_or_highlights_give_null()
    {
        Assert.Null(List.HighlightsFor("c2", "FullName"));
        Assert.Null(List.HighlightsFor("c1", "Email"));
        Assert.Null(List.HighlightsFor(null, "FullName"));
        Assert.Null(new HighlightedList().HighlightsFor("c1", "FullName"));
    }

    [Fact]
    public void a_list_without_highlight_support_gives_null()
    {
        Assert.Null(new PlainList().HighlightsFor("c1", "FullName"));
        Assert.Null(((IModelList<string>?)null).HighlightsFor("c1", "FullName"));
    }
}

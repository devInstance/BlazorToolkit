using DevInstance.BlazorToolkit.Components.DataGrid;
using Xunit;

namespace DevInstance.BlazorToolkit.Components.Tests;

public class GridLayoutTests
{
    [Fact]
    public void no_width_gives_no_style()
    {
        Assert.Equal("", GridLayout.HeaderWidthStyle(null, ["20%", null]));
    }

    [Fact]
    public void percent_widths_are_scaled_to_add_up_to_100()
    {
        // Two columns hidden: 20% + 30% visible → 40% and 60%.
        Assert.Equal("width: 40%;", GridLayout.HeaderWidthStyle("20%", ["20%", "30%"]));
        Assert.Equal("width: 60%;", GridLayout.HeaderWidthStyle("30%", ["20%", "30%"]));
    }

    [Fact]
    public void scaled_widths_use_the_invariant_culture_and_round_to_four_places()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        try
        {
            Assert.Equal("width: 33.3333%;", GridLayout.HeaderWidthStyle("10%", ["10%", "10%", "10%"]));
            Assert.Equal("width: 23.5%;", GridLayout.HeaderWidthStyle("23.50%", ["23.50%", "76.50%"]));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }

    [Fact]
    public void a_mix_of_units_is_left_as_is()
    {
        Assert.Equal("width: 20%;", GridLayout.HeaderWidthStyle("20%", ["20%", "120px"]));
        Assert.Equal("width: 120px;", GridLayout.HeaderWidthStyle("120px", ["20%", "120px"]));
        Assert.Equal("width: 20%;", GridLayout.HeaderWidthStyle("20%", ["20%", null]));
    }

    [Fact]
    public void zero_or_unparsable_percentages_disable_scaling()
    {
        Assert.Equal("width: 20%;", GridLayout.HeaderWidthStyle("20%", ["20%", "0%"]));
        Assert.Equal("width: 20%;", GridLayout.HeaderWidthStyle("20%", ["20%", "abc%"]));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "" }, null)]
    [InlineData(new[] { "Name" }, "Name")]
    [InlineData(new[] { "+Name" }, "Name")]
    [InlineData(new[] { "-Name", "Date" }, "Name")]
    public void sort_field_is_the_first_entry_without_its_direction(string[]? sortOrder, string? expected)
    {
        Assert.Equal(expected, GridLayout.SortField(sortOrder));
    }

    [Fact]
    public void group_keys_sort_by_value_with_nulls_last()
    {
        var keys = new object?[] { 10, null, 2, 33 };
        Assert.Equal(new object?[] { 2, 10, 33, null }, keys.OrderBy(k => k, GridLayout.GroupKeyComparer.Instance));
    }

    [Fact]
    public void group_keys_of_different_types_sort_by_text_ignoring_case()
    {
        var keys = new object?[] { "beta", 5, "Alpha", null };
        Assert.Equal(new object?[] { 5, "Alpha", "beta", null }, keys.OrderBy(k => k, GridLayout.GroupKeyComparer.Instance));
    }

    [Fact]
    public void dates_group_in_date_order_not_text_order()
    {
        var keys = new object?[] { new DateOnly(2026, 10, 2), new DateOnly(2026, 9, 30) };
        Assert.Equal(new object?[] { new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 2) }, keys.OrderBy(k => k, GridLayout.GroupKeyComparer.Instance));
    }
}

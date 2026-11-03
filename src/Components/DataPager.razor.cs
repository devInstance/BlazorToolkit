using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Components;

/// <summary>
/// A Blazor component for displaying pagination controls.
/// </summary>
public partial class DataPager
{
    /// <summary>
    /// Gets or sets the maximum number of page buttons to display in the pager.
    /// </summary>
    [Parameter]
    public int MaxItems { get; set; } = 15;

    /// <summary>
    /// Gets or sets the URL template for page links. Use {0} as a placeholder for the page number.
    /// </summary>
    [Parameter]
    public string? UrlTemplate { get; set; } = null;

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    [Parameter]
    public int PagesCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the currently selected page index (zero-based).
    /// </summary>
    [Parameter]
    public int SelectedPage { get; set; } = 0;

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether data loading is in progress.
    /// </summary>
    [Parameter]
    public bool InProgress { get; set; } = false;

    private IEnumerable<int> PageRange => DataPageUtils.GetPageRange(SelectedPage, PagesCount, MaxItems);

    protected async override Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void ChangePage(int newValue)
    {
        if (newValue >= 0 && newValue < PagesCount)
        {
            OnPageChanged.InvokeAsync(newValue);
        }
    }

    private string GetPageUrl(int pageNumber)
    {
        var validPage = Math.Max(0, Math.Min(pageNumber, PagesCount - 1));
        return string.Format(UrlTemplate, validPage);
    }

}

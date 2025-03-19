using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Components;

public partial class DataPager
{
    [Parameter]
    public int MaxItems { get; set; } = 15;

    [Parameter]
    public string? UrlTemplate { get; set; } = null;

    [Parameter]
    public int PagesCount { get; set; } = 0;

    [Parameter]
    public int SelectedPage { get; set; } = 0;

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

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

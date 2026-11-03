using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DevInstance.BlazorToolkit.Components.Search;

public partial class HSearchBar
{
    private bool _isOpen;

    [Parameter]
    public string Placeholder { get; set; } = "Search...";

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public EventCallback OnSearch { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? AdvancedContent { get; set; }

    private async Task HandleInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString() ?? string.Empty;
        await ValueChanged.InvokeAsync(Value);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await OnSearch.InvokeAsync();
        }
    }

    private async Task HandleSearchClick()
    {
        await OnSearch.InvokeAsync();
    }

    private void ToggleDropdown()
    {
        _isOpen = !_isOpen;
    }

    private void CloseDropdown()
    {
        _isOpen = false;
    }

    private async Task HandleDropdownSearch()
    {
        _isOpen = false;
        await OnSearch.InvokeAsync();
    }
}

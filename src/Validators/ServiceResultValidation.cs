using DevInstance.BlazorToolkit.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace DevInstance.BlazorToolkit.Validators;


/// <summary>
/// A Blazor component that integrates FluentValidation with EditContext.
/// source: https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-9.0
/// </summary>
public class ServiceResultValidation : ComponentBase
{
    private ValidationMessageStore? messageStore;

    [CascadingParameter]
    protected EditContext? CurrentEditContext { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ServiceResultValidation)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. " +
                $"For example, you can use {nameof(ServiceResultValidation)} " +
                $"inside an {nameof(EditForm)}.");
        }

        messageStore = new(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += (s, e) => messageStore?.Clear();
        CurrentEditContext.OnFieldChanged += (s, e) => messageStore?.Clear(e.FieldIdentifier);
    }

    /// <summary>
    /// Displays validation errors from service action errors in the form.
    /// </summary>
    /// <param name="errors">The array of service action errors to display.</param>
    /// <returns><c>true</c> if any validation errors were added; otherwise, <c>false</c>.</returns>
    public bool DisplayErrors(ServiceActionError[] errors)
    {
        bool result = false;
        if (CurrentEditContext is not null)
        {
            foreach (var err in errors)
            {
                if(err.ErrorType == ServiceActionErrorType.Validation)
                {
                    messageStore?.Add(CurrentEditContext.Field(err.PropertyName), err.Message);
                    result = true;
                }
            }

            CurrentEditContext.NotifyValidationStateChanged();
        }
        return result;
    }

    /// <summary>
    /// Clears all validation error messages from the form.
    /// </summary>
    public void ClearErrors()
    {
        messageStore?.Clear();
        CurrentEditContext?.NotifyValidationStateChanged();
    }
}

/// <summary>
/// Extended service result validation component that supports custom CSS class providers for form field styling.
/// </summary>
/// <typeparam name="CssProviderType">The type of the CSS class provider to use for field styling.</typeparam>
public class ServiceResultValidationEx<CssProviderType> : ServiceResultValidation where CssProviderType : FieldCssClassProvider, new()
{
    /// <summary>
    /// Gets or sets the CSS class provider used for field styling.
    /// </summary>
    public CssProviderType CssProvider { get; set; } = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (CssProvider != null)
        {
            CurrentEditContext.SetFieldCssClassProvider(CssProvider);
        }
    }
}

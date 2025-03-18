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
    private EditContext? CurrentEditContext { get; set; }

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

    public void ClearErrors()
    {
        messageStore?.Clear();
        CurrentEditContext?.NotifyValidationStateChanged();
    }}

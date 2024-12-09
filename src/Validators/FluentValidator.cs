using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using FluentValidation.Results;

namespace DevInstance.BlazorToolkit.Validators;


/// <summary>
/// A Blazor component that integrates FluentValidation with EditContext.
/// </summary>
public class FluentValidator : ComponentBase
{
    /// <summary>
    /// The EditContext for the form being validated.
    /// </summary>
    [CascadingParameter]
    private EditContext EditContext { get; set; }

    /// <summary>
    /// The type of the validator to be used.
    /// </summary>
    [Parameter]
    public Type ValidatorType { get; set; }

    private IValidator Validator;
    private ValidationMessageStore ValidationMessageStore;

    /// <summary>
    /// The service provider used to resolve the validator.
    /// </summary>
    [Inject]
    private IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Sets parameters supplied by the component's parent in the render tree.
    /// </summary>
    /// <param name="parameters">The parameters to set.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Keep a reference to the original values so we can check if they have changed
        EditContext previousEditContext = EditContext;
        Type previousValidatorType = ValidatorType;

        await base.SetParametersAsync(parameters);

        if (EditContext == null)
            throw new NullReferenceException($"{nameof(FluentValidator)} must be placed within an EditForm");

        if (ValidatorType == null)
            throw new NullReferenceException($"{nameof(ValidatorType)} must be specified.");

        if (!typeof(IValidator).IsAssignableFrom(ValidatorType))
            throw new ArgumentException($"{ValidatorType.Name} must implement {typeof(IValidator).FullName}");

        if (ValidatorType != previousValidatorType)
            ValidatorTypeChanged();

        // If the EditForm.Model changes then we get a new EditContext
        // and need to hook it up
        if (EditContext != previousEditContext)
            EditContextChanged();
    }

    private void ValidatorTypeChanged()
    {
        Validator = (IValidator)ServiceProvider.GetService(ValidatorType);
    }

    private void EditContextChanged()
    {
        ValidationMessageStore = new ValidationMessageStore(EditContext);
        HookUpEditContextEvents();
    }

    private void HookUpEditContextEvents()
    {
        EditContext.OnValidationRequested += ValidationRequested;
        EditContext.OnFieldChanged += FieldChanged;
    }

    private async void ValidationRequested(object sender, ValidationRequestedEventArgs args)
    {
        ValidationMessageStore.Clear();
        var validationContext = new ValidationContext<object>(EditContext.Model);
        ValidationResult result = await Validator.ValidateAsync(validationContext);
        AddValidationResult(EditContext.Model, result);
    }

    /// <summary>
    /// Adds validation results to the ValidationMessageStore.
    /// </summary>
    /// <param name="model">The model being validated.</param>
    /// <param name="validationResult">The validation results.</param>
    public void AddValidationResult(object model, ValidationResult validationResult)
    {
        foreach (ValidationFailure error in validationResult.Errors)
        {
            var fieldIdentifier = new FieldIdentifier(model, error.PropertyName);
            ValidationMessageStore.Add(fieldIdentifier, error.ErrorMessage);
        }
        EditContext.NotifyValidationStateChanged();
    }

    private async void FieldChanged(object sender, FieldChangedEventArgs args)
    {
        FieldIdentifier fieldIdentifier = args.FieldIdentifier;
        ValidationMessageStore.Clear(fieldIdentifier);

        var propertiesToValidate = new string[] { fieldIdentifier.FieldName };
        var fluentValidationContext = new ValidationContext<object>(
            instanceToValidate: fieldIdentifier.Model,
            propertyChain: new FluentValidation.Internal.PropertyChain(),
            validatorSelector: new FluentValidation.Internal.MemberNameValidatorSelector(propertiesToValidate)
        );

        ValidationResult result = await Validator.ValidateAsync(fluentValidationContext);

        AddValidationResult(fieldIdentifier.Model, result);
    }
}

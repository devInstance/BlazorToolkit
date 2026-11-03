using Microsoft.AspNetCore.Components.Forms;

namespace DevInstance.BlazorToolkit.Validators;

/// <summary>
/// Provides Bootstrap CSS classes for form field validation states.
/// </summary>
public class BootstrapFieldCssClassProvider : FieldCssClassProvider
{
    /// <summary>
    /// Returns the CSS class string for a field based on its validation state.
    /// </summary>
    /// <param name="editContext">The edit context for the form.</param>
    /// <param name="fieldIdentifier">The identifier of the field.</param>
    /// <returns>A CSS class string including Bootstrap validation classes (is-valid, is-invalid, modified).</returns>
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
        if (editContext.IsModified(fieldIdentifier))
        {
            return isValid ? "modified is-valid" : "modified is-invalid";
        }
        return isValid ? "is-valid" : "is-invalid";
    }
}

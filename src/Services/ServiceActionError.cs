namespace DevInstance.BlazorToolkit.Services;

public enum ServiceActionErrorType
{
    Unknown,
    General,
    Validation,
}

/// <summary>
/// Represents an error that occurs during a service action.
/// </summary>
public class ServiceActionError
{

    public ServiceActionErrorType ErrorType { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    public string PropertyName { get; set; }
}

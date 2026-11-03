namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Specifies the type of error that occurred during a service action.
/// </summary>
public enum ServiceActionErrorType
{
    /// <summary>
    /// An unknown or unspecified error type.
    /// </summary>
    Unknown,

    /// <summary>
    /// An error caused by an unhandled exception thrown during the service call.
    /// The originating exception is available on <see cref="ServiceActionError.Exception"/>.
    /// </summary>
    Exception,

    /// <summary>
    /// A general error not related to validation.
    /// </summary>
    General,

    /// <summary>
    /// A validation error indicating invalid input data.
    /// </summary>
    Validation,
}

/// <summary>
/// Represents an error that occurs during a service action.
/// </summary>
public class ServiceActionError
{
    /// <summary>
    /// Gets or sets the type of error that occurred.
    /// </summary>
    public ServiceActionErrorType ErrorType { get; set; } = ServiceActionErrorType.Unknown;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the exception that caused this error, when the error originated from an
    /// unhandled exception during the service call. Populated when <see cref="ErrorType"/> is
    /// <see cref="ServiceActionErrorType.Exception"/>; otherwise <c>null</c>.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// Gets or sets the name of the property associated with the error, typically used for validation errors.
    /// </summary>
    public string PropertyName { get; set; }
}

namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Class to hold the result of the service action.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public class ServiceActionResult<T>
{
    /// <summary>
    /// Gets or sets the result of the service action.
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the service action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the errors that occurred during the service action.
    /// </summary>
    public ServiceActionError[]? Errors { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is authorized to perform the service action.
    /// </summary>
    public bool IsAuthorized { get; set; }
}

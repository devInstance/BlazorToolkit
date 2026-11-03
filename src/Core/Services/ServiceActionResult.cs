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

    /// <summary>
    /// Creates a successful service action result with the specified result value.
    /// </summary>
    /// <param name="result">The result value.</param>
    /// <returns>A successful <see cref="ServiceActionResult{T}"/> containing the result.</returns>
    public static ServiceActionResult<T> OK(T result)
    {
        return new ServiceActionResult<T>
        {
            Result = result,
            Success = true,
            IsAuthorized = true
        };
    }

    /// <summary>
    /// Creates a failed service action result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="ServiceActionResult{T}"/> containing the error.</returns>
    public static ServiceActionResult<T> Failed(ServiceActionError error)
    {
        return new ServiceActionResult<T>
        {
            Errors = [error],
            Result = default(T),
            Success = false,
            IsAuthorized = true
        };
    }

    /// <summary>
    /// Creates a failed service action result with the specified error message.
    /// </summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <returns>A failed <see cref="ServiceActionResult{T}"/> containing a general error with the message.</returns>
    public static ServiceActionResult<T> Failed(string message)
    {
        return new ServiceActionResult<T>
        {
            Errors = new ServiceActionError[] { new ServiceActionError { ErrorType = ServiceActionErrorType.General, Message = message } },
            Result = default(T),
            Success = false,
            IsAuthorized = true
        };
    }

    /// <summary>
    /// Creates a failed service action result indicating the user is not authorized.
    /// </summary>
    /// <returns>A failed <see cref="ServiceActionResult{T}"/> with <see cref="IsAuthorized"/> set to <c>false</c>.</returns>
    public static ServiceActionResult<T> Unauthorized()
    {
        return new ServiceActionResult<T>
        {
            Result = default(T),
            Success = false,
            IsAuthorized = false
        };
    }

}

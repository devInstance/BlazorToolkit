namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Class to hold the result of the service action
/// </summary>
/// <typeparam name="T"></typeparam>
public class ServiceActionResult<T>
{
    public T? Result { get; set; }

    public bool Success { get; set; }

    public ServiceActionError[]? Errors { get; set; }

    public bool IsAuthorized { get; set; }
}

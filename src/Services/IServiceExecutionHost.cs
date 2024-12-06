namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Interface for the service execution host. This should be 
/// implemented by the component to handle the service call 
/// status and errors.
/// </summary>
public interface IServiceExecutionHost
{
    /// <summary>
    /// Error message from the service call (when IsError is true)
    /// </summary>
    string ErrorMessage { get; set; }
    /// <summary>
    /// Flag to indicate if the service call has an error
    /// </summary>
    bool IsError { get; set; }
    /// <summary>
    /// Flag to indicate if the service call is in progress
    /// </summary>
    bool InProgress { get; set; }

    /// <summary>
    /// The implementation of this method should navigate to the login page
    /// </summary>
    void ShowLogin();
    /// <summary>
    /// The implementation of this method should call the StateHasChanged method to re-render the page
    /// </summary>
    void StateHasChanged();
}

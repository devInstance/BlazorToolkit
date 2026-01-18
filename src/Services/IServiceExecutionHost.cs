using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Specifies the type of service execution operation being performed.
/// </summary>
public enum ServiceExecutionType
{
    /// <summary>
    /// No service operation is in progress.
    /// </summary>
    None,

    /// <summary>
    /// A read operation is in progress (e.g., fetching data).
    /// </summary>
    Reading,

    /// <summary>
    /// A submit operation is in progress (e.g., saving data).
    /// </summary>
    Submitting
}

/// <summary>
/// Interface for the service execution host. This should be 
/// implemented by the component to handle the service call 
/// status and errors.
/// </summary>
public interface IServiceExecutionHost/* : IDisposable */
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
    /// Flag to indicate if the service call is in progress.
    /// </summary>
    bool InProgress { get; set; }

    /// <summary>
    /// Gets or sets the current state of the service execution.
    /// </summary>
    ServiceExecutionType ServiceState { get; set; }

    /// <summary>
    /// The implementation of this method should navigate to the login page
    /// </summary>
    void ShowLogin();

    /// <summary>
    /// The implementation of this method should call the StateHasChanged method to re-render the page
    /// </summary>
    void StateHasChanged();

    /// <summary>
    /// Gets the persistent component state used for preserving state across prerendering.
    /// </summary>
    PersistentComponentState ComponentState { get; }

    /// <summary>
    /// Gets the dictionary used for storing state values that can be persisted.
    /// </summary>
    Dictionary<string, string> State { get; }
}

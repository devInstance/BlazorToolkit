using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Represents the context for a service call, containing handlers and callbacks for various stages of execution.
/// </summary>
/// <typeparam name="T">The type of the result returned by the service call.</typeparam>
public class CallContext<T>
{
    /// <summary>
    /// Gets or sets the handler that performs the asynchronous service call.
    /// </summary>
    public PerformAsyncCallHandler<T> Handler { get; set; }

    /// <summary>
    /// Gets or sets the callback to execute when the service call succeeds.
    /// </summary>
    public Action<T> Success { get; set; } = null;

    /// <summary>
    /// Gets or sets the key used for persisting and restoring state across prerendering.
    /// </summary>
    public string StateKey { get; set; } = null;

    /// <summary>
    /// Gets or sets the asynchronous callback to execute when the service call succeeds.
    /// </summary>
    public Func<T, Task> SuccessAsync { get; set; } = null;

    /// <summary>
    /// Gets or sets the error handler callback invoked when the service call fails.
    /// </summary>
    public ErrorCallHandler Error { get; set; } = null;

    /// <summary>
    /// Gets or sets the callback to execute before the service call begins.
    /// </summary>
    public Action Before { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether progress indication should be enabled during the service call.
    /// </summary>
    public bool EnableProgress { get; set; } = true;
}

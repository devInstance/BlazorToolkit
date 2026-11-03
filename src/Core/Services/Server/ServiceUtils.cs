using DevInstance.LogScope;
using System;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Services.Server;

/// <summary>
/// Utility class for handling service calls with logging and error handling.
/// </summary>
public static class ServiceUtils
{
    /// <summary>
    /// Delegate for asynchronous service handlers.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the service handler.</typeparam>
    /// <param name="log">The logging scope.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of type T.</returns>
    public delegate Task<T> ServiceHandlerAsync<T>(IScopeLog log);

    /// <summary>
    /// Handles a service call asynchronously with logging and error handling.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the service handler.</typeparam>
    /// <param name="handler">The service handler to execute.</param>
    /// <param name="log">The logging scope.</param>
    /// <returns>A task that represents the asynchronous operation, containing a ServiceActionResult of type T.</returns>
    public static async Task<ServiceActionResult<T>> HandleServiceCallAsync<T>(ServiceHandlerAsync<T> handler, IScopeLog log = null)
    {
        using (var l = log.TraceScope(nameof(ServiceUtils)).TraceScope())
        {
            try
            {
                return ServiceActionResult<T>.OK(await handler(l));
            }
            catch (Exception ex)
            {
                l.E(ex);
                return ServiceActionResult<T>.Failed(ex.Message);
            }
        }
    }
}

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
    /// <param name="log">The logging scope.</param>
    /// <param name="handler">The service handler to execute.</param>
    /// <returns>A task that represents the asynchronous operation, containing a ServiceActionResult of type T.</returns>
    public static async Task<ServiceActionResult<T>> HandleServiceCallAsync<T>(IScopeLog log, ServiceHandlerAsync<T> handler)
    {
        using (var l = log.TraceScope("ServiceUtils").TraceScope())
        {
            try
            {
                return new ServiceActionResult<T>
                {
                    Result = await handler(l),
                    Success = true,
                    IsAuthorized = true,
                };
            }
            catch (Exception ex)
            {
                l.E(ex);
                return new ServiceActionResult<T>
                {
                    Success = false,
                    Errors = new ServiceActionError[] { new ServiceActionError { Message = ex.Message } },
                    Result = default!
                };
            }
        }
    }
}

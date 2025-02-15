using DevInstance.LogScope;
using System;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Provides extension methods for service call execution.
/// </summary>
public static class ServiceCallExtensions
{
    /// <summary>
    /// Begins a service call and returns a ServiceExecutionHandler.
    /// </summary>
    /// <param name="host">The service execution host.</param>
    /// <param name="log">Optional log scope.</param>
    /// <returns>A ServiceExecutionHandler instance.</returns>
    public static ServiceExecutionHandler BeginServiceCall(this IServiceExecutionHost host, 
                                                            ServiceExecutionType executionType = ServiceExecutionType.Read, 
                                                            IScopeLog log = null)
    {
        return new ServiceExecutionHandler(log, host, executionType);
    }

    /// <summary>
    /// Executes an asynchronous service call.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="host">The service execution host.</param>
    /// <param name="handler">The handler to perform the asynchronous call.</param>
    /// <param name="success">Optional success callback.</param>
    /// <param name="sucessAsync">Optional asynchronous success callback.</param>
    /// <param name="error">Optional error callback.</param>
    /// <param name="before">Optional callback to execute before the service call.</param>
    /// <param name="enableProgress">Flag to enable progress indication.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ServiceCallAsync<T>(this IServiceExecutionHost host, 
                                                        ServiceExecutionType executionType, 
                                                        PerformAsyncCallHandler<T> handler, 
                                                        Action<T> success = null, 
                                                        Func<T, Task> sucessAsync = null, 
                                                        Action<ServiceActionError[]> error = null, 
                                                        Action before = null, 
                                                        bool enableProgress = true)
    {
        await host.BeginServiceCall(executionType)
            .DispatchCall<T>(handler, success, sucessAsync, error, before, enableProgress)
            .ExecuteAsync();
    }

    /// <summary>
    /// Executes an asynchronous service read call.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="host">The service execution host.</param>
    /// <param name="handler">The handler to perform the asynchronous call.</param>
    /// <param name="success">Optional success callback.</param>
    /// <param name="sucessAsync">Optional asynchronous success callback.</param>
    /// <param name="error">Optional error callback.</param>
    /// <param name="before">Optional callback to execute before the service call.</param>
    /// <param name="enableProgress">Flag to enable progress indication.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ServiceReadAsync<T>(this IServiceExecutionHost host,
                                                        PerformAsyncCallHandler<T> handler,
                                                        Action<T> success = null,
                                                        Func<T, Task> sucessAsync = null,
                                                        Action<ServiceActionError[]> error = null,
                                                        Action before = null,
                                                        bool enableProgress = true)
    {
        await host.BeginServiceCall(ServiceExecutionType.Read)
            .DispatchCall<T>(handler, success, sucessAsync, error, before, enableProgress)
            .ExecuteAsync();
    }

    /// <summary>
    /// Executes an asynchronous service submit call.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="host">The service execution host.</param>
    /// <param name="handler">The handler to perform the asynchronous call.</param>
    /// <param name="success">Optional success callback.</param>
    /// <param name="sucessAsync">Optional asynchronous success callback.</param>
    /// <param name="error">Optional error callback.</param>
    /// <param name="before">Optional callback to execute before the service call.</param>
    /// <param name="enableProgress">Flag to enable progress indication.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ServiceSubmitAsync<T>(this IServiceExecutionHost host,
                                                        PerformAsyncCallHandler<T> handler,
                                                        Action<T> success = null,
                                                        Func<T, Task> sucessAsync = null,
                                                        Action<ServiceActionError[]> error = null,
                                                        Action before = null,
                                                        bool enableProgress = true)
    {
        await host.BeginServiceCall(ServiceExecutionType.Submit)
            .DispatchCall<T>(handler, success, sucessAsync, error, before, enableProgress)
            .ExecuteAsync();
    }
}

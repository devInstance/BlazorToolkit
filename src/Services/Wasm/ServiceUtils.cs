using DevInstance.LogScope;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Services.Wasm;

public delegate void ResultHandler<T>(T result);


/// <summary>
/// Utility class for handling web API calls.
/// </summary>
public static class ServiceUtils
{
    /// <summary>
    /// Delegate for handling asynchronous web API calls.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the web API call.</typeparam>
    /// <param name="log">The log scope for tracing.</param>
    /// <returns>A task representing the asynchronous operation, with a result of type T.</returns>
    public delegate Task<T> WebApiHandlerAsync<T>(IScopeLog log);

    /// <summary>
    /// Handles a web API call asynchronously, providing logging and error handling.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the web API call.</typeparam>
    /// <param name="handler">The web API handler delegate.</param>
    /// <param name="log">The log scope for tracing. Optional.</param>
    /// <returns>A task representing the asynchronous operation, with a result of type ServiceActionResult<T>.</returns>
    public static async Task<ServiceActionResult<T>> HandleWebApiCallAsync<T>(WebApiHandlerAsync<T> handler, IScopeLog log = null)
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
            catch (HttpRequestException ex)
            {
                l.E(ex);
                return new ServiceActionResult<T>
                {
                    Success = false,
                    Errors = new ServiceActionError[]
                    {
                        new ServiceActionError
                        {
                            //TODO: figure out how to deliver field name for the conflict
                            Message = ex.Message
                        }
                    },
                    IsAuthorized = !(ex.StatusCode == HttpStatusCode.Unauthorized),
                    Result = default!
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

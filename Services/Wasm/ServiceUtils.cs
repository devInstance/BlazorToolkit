using DevInstance.LogScope;
using System.Net;

namespace DevInstance.BlazorToolkit.Services.Wasm;

public delegate void ResultHandler<T>(T result);

//TODO: make it utils as for server. move the base service to the client project
public static class ServiceUtils
{
    public delegate Task<T> WebApiHandlerAsync<T>(IScopeLog log);

    public static async Task<ServiceActionResult<T>> HandleWebApiCallAsync<T>(IScopeLog log, WebApiHandlerAsync<T> handler)
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

using DevInstance.LogScope;

namespace DevInstance.BlazorToolkit.Services.Server
{
    public static class ServiceUtils
    {
        public delegate Task<T> ServiceHandlerAsync<T>(IScopeLog log);

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
}

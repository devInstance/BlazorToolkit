using DevInstance.LogScope;

namespace DevInstance.BlazorToolkit.Services;

public static class ServiceCallExtensions
{
    public static ServiceExecutionHandler BeginServiceCall(this IServiceExecutionHost host, IScopeLog log = null)
    {
        return new ServiceExecutionHandler(log, host);
    }

    public static async Task ServiceCallAsync<T>(this IServiceExecutionHost host, PerformAsyncCallHandler<T> handler, Action<T> success = null, Func<T, Task> sucessAsync = null, Action<ServiceActionError[]> error = null, Action before = null, bool enableProgress = true)
    {
        await host.BeginServiceCall().DispatchCall<T>(handler, success, sucessAsync, error, before, enableProgress).ExecuteAsync();
    }
}

using DevInstance.BlazorToolkit.Extensions;
using DevInstance.LogScope;
using System.Text.Json;

namespace DevInstance.BlazorToolkit.Services;

/// <summary>
/// Delegate for performing an asynchronous service call that returns a ServiceActionResult.
/// </summary>
/// <typeparam name="T">The type of the result returned by the service call.</typeparam>
/// <returns>A task that represents the asynchronous operation, containing the ServiceActionResult.</returns>
public delegate Task<ServiceActionResult<T>> PerformAsyncCallHandler<T>();

public delegate bool ErrorCallHandler(ServiceActionError[] errors);

/// <summary>
/// Handles the execution of service calls, managing their progress, success, and error states.
/// </summary>
public class ServiceExecutionHandler
{
    private List<Func<Task<bool>>> tasks;
    private IServiceExecutionHost basePage;
    private ServiceExecutionType executionType = ServiceExecutionType.Reading;
    IScopeLog log;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceExecutionHandler"/> class.
    /// </summary>
    /// <param name="l">The scope log for tracing.</param>
    /// <param name="basePage">The base page that implements IServiceExecutionHost.</param>
    public ServiceExecutionHandler(IScopeLog l, IServiceExecutionHost basePage, ServiceExecutionType executionType)
    {
        if(executionType  == ServiceExecutionType.None) {
            throw new ArgumentException("Invalid execution type");
        }

        this.log = l.TraceScope($"SEHandler-" + (OperatingSystem.IsBrowser() ? "WASM" : "Server"));
        this.basePage = basePage;
        this.basePage.ErrorMessage = "";
        this.basePage.IsError = false;
        this.executionType = executionType;
        tasks = new List<Func<Task<bool>>>();
    }

    /// <summary>
    /// Dispatches a service call to be executed.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the service call.</typeparam>
    /// <param name="handler">The handler that performs the service call.</param>
    /// <param name="success">The action to execute if the service call is successful.</param>
    /// <param name="sucessAsync">The asynchronous action to execute if the service call is successful.</param>
    /// <param name="error">The action to execute if the service call encounters errors.</param>
    /// <param name="before">The action to execute before the service call.</param>
    /// <param name="enableProgress">Indicates whether to show progress during the service call.</param>
    /// <returns>The current instance of <see cref="ServiceExecutionHandler"/>.</returns>
    public ServiceExecutionHandler DispatchCall<T>(PerformAsyncCallHandler<T> handler,
                                                    Action<T> success = null,
                                                    string stateKey = null,
                                                    Func<T, Task> sucessAsync = null,
                                                    ErrorCallHandler error = null,
                                                    Action before = null,
                                                    bool enableProgress = true)
    {
        using (var l = log.TraceScope())
        {
            Func<Task<bool>> task = async () => await PerformServiceCallAsync(handler, success, stateKey, sucessAsync, error, before, enableProgress);
            tasks.Add(task);
            return this;
        }
    }

    /// <summary>
    /// Executes all dispatched service calls asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<bool> ExecuteAsync()
    {
        using (var l = log.TraceScope())
        {
            foreach (var task in tasks)
            {
                if (!await task())
                {
                    return false;
                }
            }
        }
        return true;
    }

    internal class StateGuard : IDisposable
    {
        private IServiceExecutionHost basePage;
        private readonly bool enableProgress;

        public StateGuard(IServiceExecutionHost basePage, ServiceExecutionType executionType, bool enableProgress)
        {
            this.basePage = basePage;
            this.enableProgress = enableProgress;
            basePage.ServiceState = executionType;
            basePage.ErrorMessage = "";
            basePage.IsError = false;
            if (enableProgress)
            {
                basePage.InProgress = true;
            }
            basePage.StateHasChanged();
        }
        public void Dispose()
        {
            basePage.ServiceState = ServiceExecutionType.None;
            if (enableProgress)
            {
                basePage.InProgress = false;
            }
            basePage.StateHasChanged();
        }
    }

    private async Task<bool> PerformServiceCallAsync<T>(PerformAsyncCallHandler<T> handler,
                                                        Action<T> success,
                                                        string stateKey,
                                                        Func<T, Task> sucessAsync,
                                                        ErrorCallHandler error,
                                                        Action before,
                                                        bool enableProgress)
    {
        ServiceActionResult<T> res = null;

        using (var l = log.TraceScope())
        {
            using (var g = new StateGuard(basePage, executionType, enableProgress))
            {
                if (before != null)
                {
                    before();
                }

                try
                {
                    l.T($"Try restoring state for {stateKey} -> state: {basePage.ComponentState}");
                    if (basePage.ComponentState != null 
                        && stateKey != null
                        && basePage.ComponentState.TryTakeFromJson<string>(stateKey, out var restoredValue))
                    {
                        l.T($"Restoring state for {stateKey} -> {restoredValue}");
                        res = JsonSerializer.Deserialize<ServiceActionResult<T>>(restoredValue);
                    }

                    if (res == null)
                    {
                        res = await handler();
                    }
                }
                catch (Exception ex)
                {
                    l.E(ex.Message);
                    l.I(ex.StackTrace);

                    res = new ServiceActionResult<T>
                    {
                        Errors = new ServiceActionError[] { new ServiceActionError { Message = ex.Message } },
                        Success = false,
                        IsAuthorized = true
                    };
                }

                if (res.Success)
                {
                    if (success != null)
                    {
                        success(res.Result);
                    }
                    if (sucessAsync != null)
                    {
                        await sucessAsync(res.Result);
                    }
                    l.T($"Try saving state for {stateKey} -> state: {basePage.ComponentState}");
                    if (basePage.ComponentState != null && stateKey != null)
                    {
                        basePage.State[stateKey] = res.ToJsonString();
                    }
                }
                else
                {
                    var errorMessage = "";
                    foreach (var errm in res.Errors)
                    {
                        errorMessage += errm.Message;
                    }
                    l.W(errorMessage);

                    bool showError = true;
                    if (!res.IsAuthorized)
                    {
                        basePage.ShowLogin();
                        return false;
                    }
                    else if (error != null)
                    {
                        showError = !error(res.Errors);
                    }
                    if(showError)
                    {
                        basePage.ErrorMessage = errorMessage;
                        basePage.IsError = true;
                    }
                }

                return true;
            }
        }
    }
}

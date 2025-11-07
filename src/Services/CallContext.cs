using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Services;

public class CallContext<T>
{
    public PerformAsyncCallHandler<T> Handler { get; set; }
    public Action<T> Success { get; set; } = null;
    public string StateKey { get; set; } = null;
    public Func<T, Task> SuccessAsync { get; set; } = null;
    public ErrorCallHandler Error { get; set; } = null;
    public Action Before { get; set; } = null;
    public bool EnableProgress { get; set; } = true;
}

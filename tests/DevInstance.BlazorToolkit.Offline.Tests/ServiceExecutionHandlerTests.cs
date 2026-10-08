using DevInstance.BlazorToolkit.Services;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace DevInstance.BlazorToolkit.Offline.Tests;

public class ServiceExecutionHandlerTests
{
    private sealed class HostStub : IServiceExecutionHost
    {
        public string ErrorMessage { get; set; } = "";
        public bool IsError { get; set; }
        public bool InProgress { get; set; }
        public ServiceExecutionType ServiceState { get; set; }
        public int LoginShown { get; private set; }
        public void ShowLogin() => LoginShown++;
        public void StateHasChanged() { }
        public PersistentComponentState ComponentState => null!;
        public Dictionary<string, string> State { get; } = new();
    }

    [Fact]
    public async Task Unauthorized_result_without_errors_shows_login_instead_of_throwing()
    {
        var host = new HostStub();

        var ok = await new ServiceExecutionHandler(new ScopeLogMock(), host, ServiceExecutionType.Reading)
            .DispatchCall(() => Task.FromResult(ServiceActionResult<string>.Unauthorized()))
            .ExecuteAsync();

        Assert.False(ok);
        Assert.Equal(1, host.LoginShown);
        Assert.False(host.IsError);
    }

    [Fact]
    public async Task Failed_result_without_errors_raises_an_empty_error_instead_of_throwing()
    {
        var host = new HostStub();

        var ok = await new ServiceExecutionHandler(new ScopeLogMock(), host, ServiceExecutionType.Reading)
            .DispatchCall(() => Task.FromResult(new ServiceActionResult<string> { Success = false, IsAuthorized = true }))
            .ExecuteAsync();

        Assert.False(ok);
        Assert.True(host.IsError);
        Assert.Equal(0, host.LoginShown);
    }
}

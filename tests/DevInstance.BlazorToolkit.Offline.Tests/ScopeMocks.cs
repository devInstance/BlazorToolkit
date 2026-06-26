using DevInstance.LogScope;

namespace DevInstance.BlazorToolkit.Offline.Tests;

public class ScopeManagerMock : IScopeManager
{
    public ILogProvider Provider => null!;
    public LogLevel BaseLevel => LogLevel.DEBUG;
    public IScopeLog CreateLogger(string scope) => new ScopeLogMock();
    public IScopeLog CreateLogger(string scope, LogLevel levelOverride) => new ScopeLogMock();
}

public class ScopeLogMock : IScopeLog
{
    public string Name => "";
    public LogLevel Level => LogLevel.DEBUG;
    public string Id => "id";
    public void Dispose() { }
    public void Line(LogLevel level, string message) { }
    public void Line(string message) { }
    public IScopeLog Scope(LogLevel level, string scope) => new ScopeLogMock();
    public IScopeLog Scope(string scope) => new ScopeLogMock();
}

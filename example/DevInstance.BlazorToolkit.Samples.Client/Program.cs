using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.LogScope.Extensions;
using DevInstance.LogScope.Formatters;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace DevInstance.BlazorToolkit.Samples.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Read this for configuring HTTP client behavior for your specific use case: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
        builder.Services.AddHttpClient("DevInstance.BlazorToolkit.Samples.Client", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

        builder.Services.AddScoped<HttpApiContextFactory>();

        builder.Services.AddScoped(sp => {
            var factory = sp.GetRequiredService<HttpApiContextFactory>();
            return factory.Create<TodoItem>("DevInstance.BlazorToolkit.Samples.Client", "api/todo");
        });

        builder.Services.AddConsoleScopeLogging(LogScope.LogLevel.TRACE, new DefaultFormattersOptions { ShowTimestamp = true });

        builder.Services.AddBlazorServices();

        await builder.Build().RunAsync();
    }
}

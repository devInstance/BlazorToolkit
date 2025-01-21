using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.EmployeeList.Client.Services;
using DevInstance.EmployeeList.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace DevInstance.EmployeeList.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Read this for configuring HTTP client behavior for your specific use case: https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines
        builder.Services.AddHttpClient("DevInstance.EmployeeList.Client", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

        builder.Services.AddScoped<HttpApiContextFactory>();

        builder.Services.AddScoped(sp => {
            var factory = sp.GetRequiredService<HttpApiContextFactory>();
            return factory.Create<EmployeeItem>("DevInstance.EmployeeList.Client", "api/employees");
        });

        builder.Services.AddBlazorServices();

        await builder.Build().RunAsync();
    }
}

using DevInstance.BlazorToolkit.Samples.Components;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.LogScope.Extensions;
using DevInstance.LogScope.Formatters;
using DevInstance.WebServiceToolkit.Http.Query;

namespace DevInstance.BlazorToolkit.Samples;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddControllers();
        builder.Services.AddBlazorServices();
        builder.Services.AddWebServiceToolkitQuery();
        builder.Services.AddConsoleScopeLogging(LogScope.LogLevel.TRACE, new DefaultFormattersOptions { ShowTimestamp = true });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.MapControllers();

        app.Run();
    }
}

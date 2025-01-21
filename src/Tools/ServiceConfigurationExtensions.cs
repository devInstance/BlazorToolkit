using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DevInstance.BlazorToolkit.Tools;

/// <summary>
/// Provides extension methods for configuring services in the DI container.
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Registers all classes with the BlazorServiceAttribute as services in the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <returns>The IServiceCollection with the registered services.</returns>
    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        return AddBlazorServices(services, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Registers all classes with the BlazorServiceAttribute as services in the DI container from the specified assembly.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="assembly">The assembly to scan for services.</param>
    /// <returns>The IServiceCollection with the registered services.</returns>
    public static IServiceCollection AddBlazorServices(this IServiceCollection services, Assembly assembly)
    {
        // 1. Get all types from the given assembly
        var types = assembly
           .GetTypes()
           .Where(t => t.IsClass
                       && !t.IsAbstract
                       && t.GetCustomAttribute<BlazorServiceAttribute>() != null);

        return AddScopedTypes(services, types);
    }

    /// <summary>
    /// Registers all classes with the BlazorServiceAttribute as services in the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <returns>The IServiceCollection with the registered services.</returns>
    public static IServiceCollection AddBlazorServicesMocks(this IServiceCollection services)
    {
        return AddBlazorServicesMocks(services, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Registers all classes with the BlazorServiceAttribute as services in the DI container from the specified assembly.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="assembly">The assembly to scan for services.</param>
    /// <returns>The IServiceCollection with the registered services.</returns>
    public static IServiceCollection AddBlazorServicesMocks(this IServiceCollection services, Assembly assembly)
    {
        // 1. Get all types from the given assembly
        var types = assembly
           .GetTypes()
           .Where(t => t.IsClass
                       && !t.IsAbstract
                       && t.GetCustomAttribute<BlazorServiceMockAttribute>() != null);

        return AddScopedTypes(services, types);
    }

    /// <summary>
    /// Registers the specified types as scoped services in the DI container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="types">The types to register as services.</param>
    /// <returns>The IServiceCollection with the registered services.</returns>
    internal static IServiceCollection AddScopedTypes(IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            // 2. Get all interfaces the class implements
            var interfaces = type.GetInterfaces();

            if (interfaces.Any())
            {
                // 3a. Register each interface–class pair
                foreach (var itf in interfaces)
                {
                    services.AddScoped(itf, type);
                }
            }
            else
            {
                // 3b. If you want to allow classes with no interfaces,
                // just register the class itself
                services.AddScoped(type);
            }
        }

        return services;
    }
}

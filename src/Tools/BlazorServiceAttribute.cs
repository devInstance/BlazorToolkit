using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.BlazorToolkit.Tools;

/// <summary>
/// Marks a class as a Blazor service for automatic registration and discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlazorServiceAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the service lifetime for dependency injection.
    /// Default is Scoped.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Initializes a new instance of the BlazorServiceAttribute with default Scoped lifetime.
    /// </summary>
    public BlazorServiceAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BlazorServiceAttribute with specified lifetime.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use for dependency injection.</param>
    public BlazorServiceAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}

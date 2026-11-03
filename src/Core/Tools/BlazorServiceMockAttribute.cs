using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.BlazorToolkit.Tools;

/// <summary>
/// Marks a class as a mock implementation of a Blazor service for testing purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlazorServiceMockAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the service lifetime for dependency injection.
    /// Default is Scoped.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Initializes a new instance of the BlazorServiceMockAttribute with default Scoped lifetime.
    /// </summary>
    public BlazorServiceMockAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BlazorServiceMockAttribute with specified lifetime.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use for dependency injection.</param>
    public BlazorServiceMockAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}

namespace DevInstance.BlazorToolkit.Tools;

/// <summary>
/// Marks a class as a Blazor service for automatic registration and discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlazorServiceAttribute : Attribute
{
}

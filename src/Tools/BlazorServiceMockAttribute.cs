namespace DevInstance.BlazorToolkit.Tools;

/// <summary>
/// Marks a class as a mock implementation of a Blazor service for testing purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BlazorServiceMockAttribute : Attribute
{
}

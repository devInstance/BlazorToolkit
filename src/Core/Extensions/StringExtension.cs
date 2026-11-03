using System.Text.Json;

namespace DevInstance.BlazorToolkit.Extensions;

/// <summary>
/// Provides extension methods for string and object serialization operations.
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string ToJsonString<T>(this T value)
    {
        return JsonSerializer.Serialize(value);
    }
}

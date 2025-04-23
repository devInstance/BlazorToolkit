using System.Text.Json;

namespace DevInstance.BlazorToolkit.Extensions;

public static class StringExtension
{
    public static string ToJsonString<T>(this T value)
    {
        return JsonSerializer.Serialize(value);
    }
}

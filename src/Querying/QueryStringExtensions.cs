using DevInstance.BlazorToolkit.Http;
using DevInstance.WebServiceToolkit.Http.Query;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace DevInstance.WebServiceToolkit.Querying;

/// <summary>
/// Provides extension methods for converting plain objects to query string representations and for appending query
/// parameters to URIs.
/// </summary>
/// <remarks>The methods in this class are intended to simplify the process of serializing objects into canonical
/// query strings, as well as merging object properties into existing URIs as query parameters. Properties of the object
/// are mapped to query parameters by name, and custom naming can be specified using the [QueryNameAttribute].
/// Collections are serialized as comma-separated values. These extensions are useful for building HTTP requests or
/// generating URLs dynamically.</remarks>
public static class QueryStringExtensions
{
    public static IApiContext<T> Query<T>(this IApiContext<T> context, object? model)
    {
        if (model is null) return context;

        var pairs = ToPairs(model);
        foreach (var kv in pairs)
        {
            context = context.Parameter(kv.Key, kv.Value);
        }
        return context;
    }

    /// <summary>Convert a POCO (optionally marked with [QueryModel]) to a canonical query string starting with '?'.</summary>
    public static string ToQueryString(this object model)
    {
        var pairs = ToPairs(model);
        return pairs.Any()
            ? "?" + string.Join("&", pairs.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"))
            : string.Empty;
    }

    // ---- internals ----
    private static IEnumerable<KeyValuePair<string, string>> ToPairs(object model)
    {
        var t = model.GetType();
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetGetMethod() is null) continue;
            var name = p.GetCustomAttribute<QueryNameAttribute>()?.Name ?? p.Name;

            var value = p.GetValue(model);
            if (value is null)
            {
                // If [DefaultValue] exists and property equals default(T), you may serialize it if you want strict determinism.
                continue;
            }

            if (value is string s)
            {
                if (s.Length > 0) yield return new(name, s);
                continue;
            }

            if (value is IEnumerable seq && value is not string)
            {
                var items = new List<string>();
                foreach (var item in seq)
                {
                    if (item is null) continue;
                    items.Add(FormatOne(item));
                }
                if (items.Count > 0)
                    yield return new(name, string.Join(',', items));
                continue;
            }

            yield return new(name, FormatOne(value));
        }

        static string FormatOne(object value) => value switch
        {
#if NET7_0_OR_GREATER
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly t => t.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
#endif
            DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture), // ISO 8601 (round-trip)
            bool b => b ? "true" : "false",
            Enum e => e.ToString(), // or ToString().ToLowerInvariant()
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}

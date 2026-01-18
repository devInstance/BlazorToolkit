using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DevInstance.BlazorToolkit.Http;


/// <summary>
/// The ApiUrlBuilder class is used to construct URLs for API endpoints.
/// It allows adding path segments and query parameters to the URL.
/// <code>
/// var url = ApiUrlBuilder.Create("api/companies").Path(id).Path("employees").Query("firstname", "John");
/// Console.Write(url);
/// </code>
/// Result:
/// <code>
/// api/companies/92387/employees?firstname=John
/// </code>
/// </summary>
public class ApiUrlBuilder
{
    protected string BaseUrl { get; set; }
    /// <summary>
    /// Dictionary to store query parameters.
    /// </summary>
    protected Dictionary<string, object> _queryParameters = new Dictionary<string, object>();

    /// <summary>
    /// List to store path segments.
    /// </summary>
    protected List<string> _path = new List<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiUrlBuilder"/> class with the specified path.
    /// </summary>
    /// <param name="path">The initial path for the URL.</param>
    private ApiUrlBuilder(string path)
    {
        BaseUrl = path;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ApiUrlBuilder"/> class with the specified path.
    /// </summary>
    /// <param name="path">The initial path for the URL.</param>
    /// <returns>A new instance of the <see cref="ApiUrlBuilder"/> class.</returns>
    public static ApiUrlBuilder Create(string path)
    {
        return new ApiUrlBuilder(path);
    }

    /// <summary>
    /// Adds a query parameter to the URL.
    /// </summary>
    /// <param name="name">The name of the query parameter.</param>
    /// <param name="value">The value of the query parameter.</param>
    /// <returns>The current instance of ApiUrlBuilder.</returns>
    public ApiUrlBuilder Query(string name, object? value)
    {
        if (value != null)
        {
            _queryParameters.Add(name, value);
        }
        return this;
    }

    /// <summary>
    /// Adds a path segment to the URL.
    /// </summary>
    /// <param name="value">The path segment to add.</param>
    /// <returns>The current instance of ApiUrlBuilder.</returns>
    public ApiUrlBuilder Path(string? value)
    {
        if (value != null)
        {
            _path.Add(value);
        }
        return this;
    }

    /// <summary>
    /// Adds a fragment identifier to the URL.
    /// </summary>
    /// <param name="value">The fragment value to add.</param>
    /// <returns>The current instance of <see cref="ApiUrlBuilder"/>.</returns>
    /// <exception cref="System.NotImplementedException">This method is not yet implemented.</exception>
    public ApiUrlBuilder Fragment(string? value)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Converts the constructed URL to a string.
    /// </summary>
    /// <returns>The constructed URL as a string.</returns>
    public override string ToString()
    {
        StringBuilder result = new StringBuilder(BaseUrl);

        foreach (var item in _path)
        {
            result.Append("/").Append(item);
        }

        bool hasQuery = false;
        foreach (var pair in _queryParameters)
        {
            if (hasQuery)
            {
                result.Append("&");
            }
            else
            {
                result.Append("?");
            }
            result.Append(pair.Key).Append("=").Append(pair.Value);

            hasQuery = true;
        }

        return result.ToString();
    }
}

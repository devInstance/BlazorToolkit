using System.Text;

namespace DevInstance.BlazorToolkit.Http;

public class ApiUrlBuilder
{
    StringBuilder result;

    protected Dictionary<string, object> _queryParameters = new Dictionary<string, object>();
    protected List<string> _path = new List<string>();

    private ApiUrlBuilder(string controller)
    {
        result = new StringBuilder(controller);
    }

    public static ApiUrlBuilder Create(string controller)
    {
        return new ApiUrlBuilder(controller);
    }

    public ApiUrlBuilder Query(string name, object? value)
    {
        if (value != null)
        {
            _queryParameters.Add(name, value);
        }
        return this;
    }

    public ApiUrlBuilder Path(string? value)
    {
        if (value != null)
        {
            _path.Add(value);
        }
        return this;
    }

    public override string ToString()
    {
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

namespace DevInstance.BlazorToolkit.Http;

public class HttpApiContextFactory
{
    public static IApiContext<T> Create<T>(HttpClient http, string url)
    {
        return new HttpApiContext<T>(url, http);
    }
}

using DevInstance.WebServiceToolkit.Common.Model;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Http;

internal class HttpApiContext<T> : IApiContext<T>
{
    private enum ApiMethod
    {
        Get,
        PostTyped,
        PostObject,
        Put,
        Delete
    }

    private ApiMethod method;
    private ApiUrlBuilder apiUrlBuilder;
    private T payload;
    private object payloadObj;

    public HttpClient Http { get; }

    public HttpApiContext(string url, HttpClient http)
    {
        apiUrlBuilder = ApiUrlBuilder.Create(url);
        Http = http;
    }

    public IApiContext<T> Get(string? id = null)
    {
        method = ApiMethod.Get;
        return this;
    }

    public IApiContext<T> Post(T obj)
    {
        method = ApiMethod.PostTyped;
        payload = obj;
        return this;
    }

    public IApiContext<T> Post(object obj)
    {
        method = ApiMethod.PostObject;
        payloadObj = obj;
        return this;
    }

    public IApiContext<T> Put(string? id, T obj)
    {
        method = ApiMethod.Put;
        apiUrlBuilder.Path(id);
        payload = obj;
        return this;
    }

    public IApiContext<T> Delete(string? id)
    {
        method = ApiMethod.Delete;
        apiUrlBuilder.Path(id);
        return this;
    }

    public IApiContext<T> Url(string url)
    {
        apiUrlBuilder = ApiUrlBuilder.Create(url);
        return this;
    }

    public IApiContext<T> Url(ApiUrlBuilder url)
    {
        apiUrlBuilder = url;
        return this;
    }

    public async Task<ModelList<T>?> ExecuteListAsync()
    {
        string url = apiUrlBuilder.ToString();
        return await Http.GetFromJsonAsync<ModelList<T>>(url);
    }

    public async Task<T?> ExecuteAsync()
    {
        string url = apiUrlBuilder.ToString();
        switch(method)
        {
            case ApiMethod.Get:
                return await Http.GetFromJsonAsync<T>(url);
            case ApiMethod.PostTyped:
            {
                var result = await Http.PostAsJsonAsync(url, payload);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<T>();
            }
            case ApiMethod.PostObject:
            {
                var result = await Http.PostAsJsonAsync(url, payloadObj);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<T>();
            }
            case ApiMethod.Put:
            {
                var result = await Http.PutAsJsonAsync(url, payload);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<T>();
            }
            case ApiMethod.Delete:
            {
                var result = await Http.DeleteAsync(url);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<T>();
            }
            default:
                return default;
        }
    }

    public IApiContext<T> Parameter(string name, object value)
    {
        apiUrlBuilder.Query(name, value);
        return this;
    }

    public IApiContext<T> Parameter<F>(string name, F value)
    {
        apiUrlBuilder.Query(name, value);
        return this;
    }

    public IApiContext<T> Path(string name)
    {
        apiUrlBuilder.Path(name);
        return this;
    }

    public IApiContext<T> Fragment(string name)
    {
        apiUrlBuilder.Fragment(name);
        return this;
    }
}

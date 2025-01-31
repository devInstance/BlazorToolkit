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
        Post,
        Put,
        Delete
    }

    private ApiMethod method;
    private ApiUrlBuilder apiUrlBuilder;
    private object payload;

    public HttpClient Http { get; }

    public string Uri => apiUrlBuilder.ToString();

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
        return Post<T>(obj);
    }

    public IApiContext<T> Post<O>(O obj)
    {
        method = ApiMethod.Post;
        payload = obj;
        return this;
    }

    public IApiContext<T> Put(string? id, T obj)
    {
        return Put<T>(id, obj);
    }
    public IApiContext<T> Put<O>(string? id, O obj)
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

    public async Task<O?> ExecuteAsync<O>()
    {
        string url = apiUrlBuilder.ToString();
        switch (method)
        {
            case ApiMethod.Get:
                return await Http.GetFromJsonAsync<O>(url);
            case ApiMethod.Post:
            {
                var result = await Http.PostAsJsonAsync(url, payload);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<O>();
            }
            case ApiMethod.Put:
            {
                var result = await Http.PutAsJsonAsync(url, payload);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<O>();
            }
            case ApiMethod.Delete:
            {
                var result = await Http.DeleteAsync(url);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<O>();
            }
            default:
                return default;
        }
    }

    public async Task<ModelList<T>?> ExecuteListAsync()
    {
        return await ExecuteAsync<ModelList<T>>();
    }

    public async Task<T?> ExecuteAsync()
    {
        return await ExecuteAsync<T>();
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

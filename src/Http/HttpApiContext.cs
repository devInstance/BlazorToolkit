using DevInstance.WebServiceToolkit.Common.Model;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DevInstance.BlazorToolkit.Http;

internal class HttpApiContext<T> : HttpApiContext<string, T>, IApiContext<T>
{
    public HttpApiContext(string url, HttpClient http) : base(url, http)
    {
    }

    IApiContext<T> IApiContext<T>.Fragment(string name)
    {
        base.Fragment(name);
        return this;
    }

    IApiContext<T> IApiContext<T>.Get(string? id)
    {
        base.Get(id);
        return this;
    }

    IApiContext<T> IApiContext<T>.Parameter<F>(string name, F value)
    {
        base.Parameter<F>(name, value);
        return this;
    }

    IApiContext<T> IApiContext<T>.Path(string name)
    {
        base.Path(name);
        return this;
    }

    IApiContext<T> IApiContext<T>.Post(T obj)
    {
        base.Post(obj);
        return this;
    }

    IApiContext<T> IApiContext<T>.Post<O>(O obj)
    {
        base.Post<O>(obj);
        return this;
    }

    IApiContext<T> IApiContext<T>.Put(T obj, string? id)
    {
        base.Put(obj, id);
        return this;
    }

    IApiContext<T> IApiContext<T>.Put<O>(O obj, string? id)
    {
        base.Put<O>(obj, id);
        return this;
    }

    public IApiContext<T> Delete(string? id)
    {
        base.Delete(id);
        return this;
    }

    IApiContext<T> IApiContext<T>.Url(string url)
    {
        base.Url(url);
        return this;
    }

    IApiContext<T> IApiContext<T>.Url(ApiUrlBuilder url)
    {
        base.Url(url);
        return this;
    }
}

internal class HttpApiContext<K, T> : IApiContext<K, T>
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

    public IApiContext<K, T> Get(K? id = default)
    {
        method = ApiMethod.Get;
        if (id != null)
        {
            apiUrlBuilder.Path(id.ToString());
        }
        return this;
    }

    public IApiContext<K, T> Post(T obj)
    {
        return Post<T>(obj);
    }

    public IApiContext<K, T> Post<O>(O obj)
    {
        method = ApiMethod.Post;
        payload = obj;
        return this;
    }

    public IApiContext<K, T> Put(T obj, K? id = default)
    {
        return Put<T>(obj, id);
    }
    public IApiContext<K, T> Put<O>(O obj, K? id = default)  {
        method = ApiMethod.Put;
        if (id != null)
        {
            apiUrlBuilder.Path(id.ToString());
        }
        payload = obj;
        return this;
    }

    public IApiContext<K, T> Delete(K? id)
    {
        method = ApiMethod.Delete;
        if (id != null)
        {
            apiUrlBuilder.Path(id.ToString());
        }
        return this;
    }

    public IApiContext<K, T> Url(string url)
    {
        apiUrlBuilder = ApiUrlBuilder.Create(url);
        return this;
    }

    public IApiContext<K, T> Url(ApiUrlBuilder url)
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

    public IApiContext<K, T> Parameter(string name, object value)
    {
        apiUrlBuilder.Query(name, value);
        return this;
    }

    public IApiContext<K, T> Parameter<F>(string name, F value)
    {
        apiUrlBuilder.Query(name, value);
        return this;
    }

    public IApiContext<K, T> Path(string name)
    {
        apiUrlBuilder.Path(name);
        return this;
    }

    public IApiContext<K, T> Fragment(string name)
    {
        apiUrlBuilder.Fragment(name);
        return this;
    }
}

using DevInstance.DevCoreApp.Shared.Model;

namespace DevInstance.BlazorToolkit.Http;

public interface IApiContext<T>
{
    IApiContext<T> Get(string? id = null);
    IApiContext<T> Post(T obj);
    IApiContext<T> Put(string? id, T obj);
    IApiContext<T> Delete(string? id);

    IApiContext<T> Top(int top);
    IApiContext<T> Page(int page);
    IApiContext<T> Search(string key);
    IApiContext<T> Sort(string key, bool isAsc);

    Task<T?> ExecuteAsync();
    Task<ModelList<T>?> ListAsync();
}

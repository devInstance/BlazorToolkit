using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Http.Extensions;
using DevInstance.BlazorToolkit.Samples.QueryModel;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Client.Services;

[BlazorService]
public class TodoService : ITodoService
{
    IApiContext<TodoItem> Api { get; set; }

    public TodoService(IApiContext<TodoItem> api)
    {
        Api = api;
    }

    ModelList<TodoItem> modelList;

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(TodoQueryModel query)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                query.Include = new[] { "Value1", "Value2" };
                return await Api.Get().Query(query).ExecuteListAsync();
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> AddAsync(TodoItem newTodo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Post(newTodo).ExecuteListAsync();
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> UpdateAsync(TodoItem updatedTodo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Put(updatedTodo, updatedTodo.Id).ExecuteListAsync();
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> DeleteAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Delete(id).ExecuteListAsync();
            }
        );
    }
}

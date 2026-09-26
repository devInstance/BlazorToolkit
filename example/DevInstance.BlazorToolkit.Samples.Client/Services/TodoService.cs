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

    TodoItemList modelList;

    public async Task<ServiceActionResult<TodoItemList?>> GetItemsAsync(TodoQueryModel query)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                query.Include = new[] { "Value1", "Value2" };
                return await Api.Get().Query(query).ExecuteAsync<TodoItemList>();
            }
        );
    }

    public async Task<ServiceActionResult<TodoItemList?>> AddAsync(TodoItem newTodo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Post(newTodo).ExecuteAsync<TodoItemList>();
            }
        );
    }

    public async Task<ServiceActionResult<TodoItemList?>> UpdateAsync(TodoItem updatedTodo)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Put(updatedTodo, updatedTodo.Id).ExecuteAsync<TodoItemList>();
            }
        );
    }

    public async Task<ServiceActionResult<TodoItemList?>> DeleteAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                return await Api.Delete(id).ExecuteAsync<TodoItemList>();
            }
        );
    }
}

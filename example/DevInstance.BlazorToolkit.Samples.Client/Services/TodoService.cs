using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.WebServiceToolkit.Common.Model;
using DevInstance.BlazorToolkit.Http.Extensions;

namespace DevInstance.BlazorToolkit.Samples.Services;

[BlazorService]
public class TodoService
{
    IApiContext<TodoItem> Api { get; set; }

    public TodoService(IApiContext<TodoItem> api)
    {
        Api = api;
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(int? top, int? page, string? search)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                var api = Api.Get();
                if (top.HasValue)
                {
                    api = api.Top(top.Value);
                }
                if (page.HasValue)
                {
                    api = api.Page(page.Value);
                }
                if (String.IsNullOrEmpty(search))
                {
                    api = api.Search(search);
                }

                return await api.ExecuteListAsync();
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

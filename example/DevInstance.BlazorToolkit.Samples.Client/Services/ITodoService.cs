using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Samples.QueryModel;
using DevInstance.BlazorToolkit.Services;
using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Client.Services;

public interface ITodoService
{
    Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(TodoQueryModel query);
    Task<ServiceActionResult<ModelList<TodoItem>?>> AddAsync(TodoItem newTodo);
    Task<ServiceActionResult<ModelList<TodoItem>?>> UpdateAsync(TodoItem updatedTodo);
    Task<ServiceActionResult<ModelList<TodoItem>?>> DeleteAsync(string id);
}

using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Samples.QueryModel;
using DevInstance.BlazorToolkit.Services;
using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Client.Services;

public interface ITodoService
{
    Task<ServiceActionResult<TodoItemList?>> GetItemsAsync(TodoQueryModel query);
    Task<ServiceActionResult<TodoItemList?>> AddAsync(TodoItem newTodo);
    Task<ServiceActionResult<TodoItemList?>> UpdateAsync(TodoItem updatedTodo);
    Task<ServiceActionResult<TodoItemList?>> DeleteAsync(string id);
}

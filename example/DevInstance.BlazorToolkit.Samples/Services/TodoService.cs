using DevInstance.BlazorToolkit.Samples.Client.Services;
using DevInstance.BlazorToolkit.Samples.Data;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Server;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Services;

[BlazorService]
public class TodoService : ITodoService
{
    public TodoRepository Repository { get; }

    public TodoService(TodoRepository repository)
    {
        Repository = repository;
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> GetItemsAsync(int? top, int? page, string? search)
    {
        return await ServiceUtils.HandleServiceCallAsync(
            async (l) =>
            {
                return await Repository.GetItemsAsync(top, page, search);
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> AddAsync(TodoItem newTodo)
    {
        if (await Repository.DoesExistAsync(newTodo))
        {
            var error = new ServiceActionError
            {
                ErrorType = ServiceActionErrorType.Validation,
                PropertyName = nameof(newTodo.Title),
                Message = $"Task with '{nameof(newTodo.Title)}' already exists"
            };
            return ServiceActionResult<ModelList<TodoItem>?>.Failed(error);
        }

        return await ServiceUtils.HandleServiceCallAsync(
            async (l) =>
            {
                return await Repository.AddAsync(newTodo);
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> UpdateAsync(TodoItem updatedTodo)
    {
        return await ServiceUtils.HandleServiceCallAsync(
            async (l) =>
            {
                return await Repository.UpdateAsync(updatedTodo.Id, updatedTodo);
            }
        );
    }

    public async Task<ServiceActionResult<ModelList<TodoItem>?>> DeleteAsync(string id)
    {
        return await ServiceUtils.HandleServiceCallAsync(
            async (l) =>
            {
                return await Repository.DeleteAsync(id);
            }
        );
    }
}

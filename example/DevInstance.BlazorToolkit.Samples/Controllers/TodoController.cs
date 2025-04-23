using DevInstance.BlazorToolkit.Samples.Data;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevInstance.EmployeeList.Controllers;

[ApiController]
[Route("api/todo")]
public class TodoController
{
    public TodoRepository Repository { get; }

    public TodoController(TodoRepository repository)
    {
        Repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<ModelList<TodoItem>>> GetItemsAsync(int? top, int? page, string? search)
    {
        return await Repository.GetItemsAsync(top, page, search);
    }

    [HttpPost]
    public async Task<ActionResult<ModelList<TodoItem>>> AddAsync([FromBody()] TodoItem item)
    {
        if (await Repository.DoesExistAsync(item))
        {
            var error = new ServiceActionError 
            {
                ErrorType = ServiceActionErrorType.Validation,
                PropertyName = nameof(item.Title),
                Message = $"Task with '{nameof(item.Title)}' already exists"
            };
            return new BadRequestObjectResult(JsonSerializer.Serialize(error));
        }

        return await Repository.AddAsync(item);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult<ModelList<TodoItem>>> UpdateAsync(string id, [FromBody] TodoItem item)
    {
        return await Repository.UpdateAsync(id, item);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult<ModelList<TodoItem>>> DeleteAsync(string id)
    {
        return await Repository.DeleteAsync(id);
    }

    [HttpGet]
    [Route("error404")]
    public async Task<ActionResult<TodoItem>> GetErrorAsync()
    {
        await Repository.DelayAsync();

        return new UnauthorizedResult();
    }
}

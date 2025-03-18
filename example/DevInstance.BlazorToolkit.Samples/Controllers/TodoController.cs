using DevInstance.BlazorToolkit.Samples.Components.Pages;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Utils;
using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace DevInstance.EmployeeList.Controllers;

[ApiController]
[Route("api/todo")]

public class TodoController
{
    private static int delay = 3000;

    private static List<TodoItem> list = null;

    public TodoController()
    {
        if (list == null)
        {
            list = new List<TodoItem>();
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Buy groceries", IsCompleted = true });
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Finish Blazor tutorial", IsCompleted = false });
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Send project proposal", IsCompleted = false });
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Call dentist", IsCompleted = false });
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Pay utility bills", IsCompleted = true });
            list.Add(new TodoItem { Id = IdGenerator.New(), Title = "Schedule team meeting", IsCompleted = true });
        }
    }

    private async Task DelayAsync()
    {
        if(delay > 0)
            await Task.Delay(delay);
    }

    private ActionResult<ModelList<TodoItem>> GetList(int? page, int? itemPerPage = null)
    {
        int pageIndex = page ?? 0;
        int pageSize = itemPerPage ?? 10;

        var totalPageCount = (int)Math.Ceiling((double)list.Count / (double)pageSize);
        var result = list.Skip(pageIndex * pageSize).Take(pageSize).ToArray();
        return new ModelList<TodoItem>
        {
            Page = pageIndex,
            TotalCount = totalPageCount,
            Count = result.Length <  pageSize ? result.Length : pageSize,
            PagesCount = (int)(list.Count / pageSize),
            Items = result
        };
    }

    [HttpGet]
    public async Task<ActionResult<ModelList<TodoItem>>> GetItemsAsync(int? top, int? page, string? search)
    {
        await DelayAsync();

        return GetList(page, top);
    }

    [HttpPost]
    public async Task<ActionResult<ModelList<TodoItem>>> AddAsync([FromBody()] TodoItem item)
    {
        if (list.FindIndex(t => string.Compare(t.Title, item.Title, StringComparison.OrdinalIgnoreCase) == 0) >= 0)
        {
            var error = new ServiceActionError { ErrorType = ServiceActionErrorType.Validation, PropertyName = nameof(item.Title), Message = $"Task with '{nameof(item.Title)}' already exists" };
            return new BadRequestObjectResult(JsonSerializer.Serialize(error));
        }

        await DelayAsync();

        list.Insert(0, new TodoItem { Id = IdGenerator.New(), Title = item.Title });

        return GetList(0);
    }

    [HttpPut]
    public async Task<ActionResult<ModelList<TodoItem>>> UpdateAsync(string id, [FromBody] TodoItem item)
    {
        await DelayAsync();

        list.Find(t => t.Id == id).Title = item.Title;

        return GetList(0);
    }

    [HttpDelete]
    public async Task<ActionResult<ModelList<TodoItem>>> DeleteAsync(string id)
    {
        await DelayAsync();

        var index = list.FindIndex(t => t.Id == id);
        list.RemoveAt(index);

        return GetList(0);
    }

    [HttpGet]
    [Route("error404")]
    public async Task<ActionResult<TodoItem>> GetErrorAsync()
    {
        await DelayAsync();

        return new UnauthorizedResult();
    }
}

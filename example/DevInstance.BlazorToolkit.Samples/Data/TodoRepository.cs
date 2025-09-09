using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Utils;
using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevInstance.BlazorToolkit.Samples.Data;

[BlazorService]
public class TodoRepository
{
    private static List<TodoItem> list = null;
    private readonly int delay;

    public TodoRepository(int delay = 1000)
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

        this.delay = delay;
    }

    public async Task DelayAsync()
    {
        if (delay > 0)
            await Task.Delay(delay);
    }

    private ModelList<TodoItem> GetList(int? page, int? itemPerPage = null)
    {
        int pageIndex = page ?? 0;
        int pageSize = itemPerPage ?? 10;

        var totalPageCount = (int)Math.Ceiling((double)list.Count / (double)pageSize);
        var result = list.Skip(pageIndex * pageSize).Take(pageSize).ToArray();
        return new ModelList<TodoItem>
        {
            Page = pageIndex,
            TotalCount = totalPageCount,
            Count = result.Length < pageSize ? result.Length : pageSize,
            PagesCount = (int)(list.Count / pageSize),
            Items = result
        };
    }

    public async Task<ModelList<TodoItem>> GetItemsAsync(int? top, int? page, string? search)
    {
        await DelayAsync();

        return GetList(page, top);
    }

    public async Task<bool> DoesExistAsync(TodoItem item)
    {
        await DelayAsync();

        return list.FindIndex(t => string.Compare(t.Title, item.Title, StringComparison.OrdinalIgnoreCase) == 0) >= 0;
    }

    public async Task<ModelList<TodoItem>> AddAsync(TodoItem item)
    {
        await DelayAsync();

        list.Insert(0, new TodoItem { Id = IdGenerator.New(), Title = item.Title });

        return GetList(0);
    }

    public async Task<ModelList<TodoItem>> UpdateAsync(string id, [FromBody] TodoItem item)
    {
        await DelayAsync();

        list.Find(t => t.Id == id).IsCompleted = item.IsCompleted;

        return GetList(0);
    }

    public async Task<ModelList<TodoItem>> DeleteAsync(string id)
    {
        await DelayAsync();

        var index = list.FindIndex(t => t.Id == id);
        list.RemoveAt(index);

        return GetList(0);
    }
}

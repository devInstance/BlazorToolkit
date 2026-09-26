using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Model;

public class TodoItemList : IModelList<TodoItem>
{
    public int TotalCount { get; set; }
    public int PagesCount { get; set; }
    public int Page { get; set; }
    public int Count { get; set; }
    public string[] SortOrder { get; set; }
    public string Search { get; set; }
    public TodoItem[] Items { get; set; }
}

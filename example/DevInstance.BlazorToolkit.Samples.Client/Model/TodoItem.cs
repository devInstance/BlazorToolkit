using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Model;

public class TodoItem : IModelItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Model;

public class TodoItem : ModelItem
{
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

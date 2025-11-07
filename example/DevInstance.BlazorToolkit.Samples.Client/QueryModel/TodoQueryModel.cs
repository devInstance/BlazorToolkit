using DevInstance.WebServiceToolkit.Http.Query;
using System.ComponentModel;

namespace DevInstance.BlazorToolkit.Samples.QueryModel;

[QueryModel]
public class TodoQueryModel
{
    [DefaultValue(0)]
    public int? Top { get; set; }
    
    [DefaultValue(10)]
    public int? Page { get; set; }
    
    public string? Search { get; set; }
    
    public string[]? Include { get; set; }
}

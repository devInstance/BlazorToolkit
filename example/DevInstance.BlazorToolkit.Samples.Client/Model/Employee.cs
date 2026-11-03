using DevInstance.WebServiceToolkit.Common.Model;

namespace DevInstance.BlazorToolkit.Samples.Model;

public class Employee : IModelItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public decimal Salary { get; set; }
}

/// <summary>
/// A list response that also carries search highlights. The server fills <see cref="Highlights"/>;
/// HDataGrid renders them for columns that set <c>HighlightField</c>.
/// </summary>
public class EmployeeList : IModelList<Employee>, IModelListHighlights
{
    public int TotalCount { get; set; }
    public int PagesCount { get; set; }
    public int Page { get; set; }
    public int Count { get; set; }
    public string[] SortOrder { get; set; } = [];
    public string Search { get; set; } = string.Empty;
    public Employee[] Items { get; set; } = [];
    public Dictionary<string, Dictionary<string, HighlightRange[]>>? Highlights { get; set; }
}

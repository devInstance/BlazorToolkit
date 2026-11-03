using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.WebServiceToolkit.Common.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DevInstance.BlazorToolkit.Samples.Data;

/// <summary>
/// In-memory stand-in for a database: searches, sorts and pages the employees, and computes search
/// highlights next to the search so they always agree with what was found.
/// </summary>
[BlazorService(ServiceLifetime.Singleton)]
public class EmployeeRepository
{
    private const int Delay = 400; // long enough to see HDataGrid's loading skeleton

    private static readonly string[] FirstNames = ["Ada", "Grace", "Linus", "Margaret", "Alan", "Barbara", "Dennis", "Frances", "Ken", "Radia", "John", "Katherine"];
    private static readonly string[] LastNames = ["Lovelace", "Hopper", "Torvalds", "Hamilton", "Turing", "Liskov", "Ritchie", "Allen", "Thompson", "Perlman", "Backus", "Johnson"];
    private static readonly string[] Departments = ["Engineering", "Sales", "Support", "Finance", "Marketing"];
    private static readonly string[] Cities = ["Honolulu", "Seattle", "Austin", "Boston", "Denver", "Portland"];

    private static readonly IReadOnlyList<Employee> All = Enumerable.Range(0, 87).Select(i => new Employee
    {
        Id = $"e{i + 1}",
        Name = $"{FirstNames[i % FirstNames.Length]} {LastNames[(i * 7) % LastNames.Length]}",
        Department = Departments[(i * 3) % Departments.Length],
        City = Cities[(i * 5) % Cities.Length],
        HireDate = new DateOnly(2015, 1, 1).AddDays(i * 41),
        Salary = 60000 + (i * 1373) % 90000,
    }).ToList();

    public async Task<EmployeeList> GetItemsAsync(int? top, int? page, string? search, string? sortBy, bool isAscending)
    {
        await Task.Delay(Delay);

        var pageSize = Math.Max(1, top ?? 10);
        IEnumerable<Employee> items = All;

        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(e => Contains(e.Name, search) || Contains(e.City, search));

        if (!string.IsNullOrEmpty(sortBy))
        {
            // sortBy is the grid column's Field
            Func<Employee, object> key = sortBy switch
            {
                nameof(Employee.Department) => e => e.Department,
                nameof(Employee.City) => e => e.City,
                nameof(Employee.HireDate) => e => e.HireDate,
                nameof(Employee.Salary) => e => e.Salary,
                _ => e => e.Name,
            };
            items = isAscending ? items.OrderBy(key) : items.OrderByDescending(key);
        }

        var filtered = items.ToList();
        var pagesCount = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)pageSize));
        var pageIndex = Math.Clamp(page ?? 0, 0, pagesCount - 1);
        var pageItems = filtered.Skip(pageIndex * pageSize).Take(pageSize).ToArray();

        return new EmployeeList
        {
            TotalCount = filtered.Count,
            PagesCount = pagesCount,
            Page = pageIndex,
            Count = pageItems.Length,
            // HDataGrid shows the sort arrow from the first entry; a leading '-' means descending
            SortOrder = string.IsNullOrEmpty(sortBy) ? [] : [(isAscending ? "" : "-") + sortBy],
            Search = search ?? string.Empty,
            Items = pageItems,
            // item id -> property name -> match ranges; read by columns with HighlightField
            Highlights = string.IsNullOrWhiteSpace(search) ? null : pageItems.ToDictionary(e => e.Id, e => new Dictionary<string, HighlightRange[]>
            {
                [nameof(Employee.Name)] = Matches(e.Name, search),
                [nameof(Employee.City)] = Matches(e.City, search),
            }),
        };
    }

    private static bool Contains(string value, string search) =>
        value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static HighlightRange[] Matches(string value, string search)
    {
        var ranges = new List<HighlightRange>();
        for (var i = value.IndexOf(search, StringComparison.OrdinalIgnoreCase); i >= 0;
             i = value.IndexOf(search, i + search.Length, StringComparison.OrdinalIgnoreCase))
        {
            ranges.Add(new HighlightRange(i, search.Length));
        }
        return ranges.ToArray();
    }
}

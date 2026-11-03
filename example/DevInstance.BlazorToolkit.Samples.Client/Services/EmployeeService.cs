using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Http.Extensions;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.BlazorToolkit.Tools;

namespace DevInstance.BlazorToolkit.Samples.Client.Services;

public interface IEmployeeService
{
    Task<ServiceActionResult<EmployeeList?>> GetItemsAsync(int pageSize, int page, string? search, string? sortBy, bool isAscending);
}

[BlazorService]
public class EmployeeService : IEmployeeService
{
    private readonly IApiContext<Employee> api;

    public EmployeeService(IApiContext<Employee> api)
    {
        this.api = api;
    }

    public async Task<ServiceActionResult<EmployeeList?>> GetItemsAsync(int pageSize, int page, string? search, string? sortBy, bool isAscending)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                // GET api/employees?top=..&page=..&search=..&sortBy=..&asc=..
                var request = api.Get().Top(pageSize).Page(page);
                if (!string.IsNullOrWhiteSpace(search))
                    request = request.Search(search);
                if (!string.IsNullOrEmpty(sortBy))
                    request = request.Sort(sortBy, isAscending);
                return await request.ExecuteAsync<EmployeeList>();
            }
        );
    }
}

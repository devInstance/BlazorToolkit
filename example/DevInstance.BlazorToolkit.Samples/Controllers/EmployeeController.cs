using DevInstance.BlazorToolkit.Samples.Data;
using DevInstance.BlazorToolkit.Samples.Model;
using Microsoft.AspNetCore.Mvc;

namespace DevInstance.BlazorToolkit.Samples.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeRepository repository;

    public EmployeeController(EmployeeRepository repository)
    {
        this.repository = repository;
    }

    // Parameter names match the IApiContext extensions: .Top() .Page() .Search() .Sort(key, isAsc)
    [HttpGet]
    public async Task<ActionResult<EmployeeList>> GetItemsAsync(
        [FromQuery] int? top, [FromQuery] int? page, [FromQuery] string? search,
        [FromQuery] string? sortBy, [FromQuery] bool? asc)
    {
        return await repository.GetItemsAsync(top, page, search, sortBy, asc ?? true);
    }
}

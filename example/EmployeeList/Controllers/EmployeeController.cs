using DevInstance.BlazorToolkit.Model;
using DevInstance.EmployeeList.Model;
using Microsoft.AspNetCore.Mvc;

namespace DevInstance.EmployeeList.Controllers;

[ApiController]
[Route("api/employees")]

public class EmployeeController
{
    [HttpGet]
    public async Task<ActionResult<ModelList<EmployeeItem>>> GetItemsAsync(int? top, int? page, string? search)
    {
        return new ModelList<EmployeeItem>
        {
            Page = 0,
            TotalCount = 2,
            Count = 2,
            PagesCount = 1,
            Items =
            [
                new EmployeeItem { Name = "John Doe" },
                new EmployeeItem { Name = "Jane Doe" }
            ]
        };
    }

}

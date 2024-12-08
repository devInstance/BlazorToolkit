using DevInstance.BlazorToolkit.Model;
using DevInstance.BlazorToolkit.Utils;
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
                new EmployeeItem { Id = IdGenerator.New(), Name = "John Doe" },
                new EmployeeItem { Id = IdGenerator.New(), Name = "Jane Doe" }
            ]
        };
    }

}

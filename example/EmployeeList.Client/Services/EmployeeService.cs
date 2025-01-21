using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Model;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.EmployeeList.Model;

namespace DevInstance.EmployeeList.Client.Services;

public class EmployeeService
{
    IApiContext<EmployeeItem> Api { get; set; }

    public EmployeeService(IApiContext<EmployeeItem> api)
    {
        Api = api;
    }

    public async Task<ServiceActionResult<ModelList<EmployeeItem>?>> GetItemsAsync(int? top, int? page, string? search)
    {
        await Task.Delay(5000);
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                var api = Api.Get();
                if (top.HasValue)
                {
                    api = api.Top(top.Value);
                }
                if (page.HasValue)
                {
                    api = api.Page(page.Value);
                }
                if (String.IsNullOrEmpty(search))
                {
                    api = api.Search(search);
                }

                return await api.ListAsync();
            }
        );
    }

}

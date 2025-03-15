using DevInstance.BlazorToolkit.Http;
using DevInstance.BlazorToolkit.Services;
using DevInstance.BlazorToolkit.Services.Wasm;
using DevInstance.BlazorToolkit.Tools;
using DevInstance.BlazorToolkit.Samples.Model;
using DevInstance.WebServiceToolkit.Common.Model;
using DevInstance.BlazorToolkit.Http.Extensions;

namespace DevInstance.BlazorToolkit.Samples.Services;

[BlazorService]
public class EmployeeService
{
    IApiContext<EmployeeItem> Api { get; set; }

    public EmployeeService(IApiContext<EmployeeItem> api)
    {
        Api = api;
    }

    public async Task<ServiceActionResult<ModelList<EmployeeItem>?>> GetItemsAsync(int? top, int? page, string? search)
    {
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

                return await api.ExecuteListAsync();
            }
        );
    }

}

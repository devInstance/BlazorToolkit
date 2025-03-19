# Blazor Toolkit
Blazor Toolkit is a comprehensive set of tools designed to enhance the development of Blazor applications. It provides a suite of utilities and services that streamline common tasks, allowing developers to focus on building rich, interactive web applications.

## Features
- **Network Communication Tools:** Simplify network operations such as making REST API calls. BlazorToolkit offers easy-to-use methods for handling HTTP requests and responses, reducing boilerplate code and potential errors.

- **Middleware Services:** Implement middleware logic as services to abstract complex processes from the UI layer. This promotes a clean separation of concerns, making your application more modular and maintainable.

- **Form Validators:** Integrate robust form validation mechanisms to ensure data integrity. The toolkit includes flexible validators that can be easily applied to your forms, providing immediate feedback to users and enhancing the overall user experience.

## Installation
Blazor Toolkit package is available on NuGet (https://www.nuget.org/packages/DevInstance.BlazorToolkit/) and can be installed using the following command:

**Power shell:**

```bash
dotnet add package DevInstance.BlazorToolkit
```

**Package manager**:

```bash
Install-Package DevInstance.BlazorToolkit
```

## Examples

Create a new Blazor WebAssembly project. Add new service class `EmployeeService.cs` and register it in `Program.cs`. Add new razor component `Home.razor` and inject `EmployeeService` into it. Use `IServiceExecutionHost` interface to handle service execution state.

`EmployeeService.cs:`
```csharp
[BlazorService]
public class EmplyeeService
{
    IApiContext<EmployeeItem> Api { get; set; }

    public EmplyeeService(IApiContext<EmployeeItem> api)
    {
        Api = api;
    }

    public async Task<ServiceActionResult<ModelList<EmployeeItem>?>> GetItemsAsync(int? top, int? page)
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
                return await api.ListAsync();
            }
        );
    }
}
```

Register service in DI container in Program.cs:
`Program.cs:`
```csharp

static async Task Main(string[] args)
{
    ...
    builder.Services.AddBlazorServices();
    ...
}
```

Add new razor component `Home.razor` and inject `EmployeeService` into it. Use `IServiceExecutionHost` interface to handle service execution state. `BlazorToolkitPageLayout` implement all necessary states like loading and error handling.

`Home.razor:`
```csharp
@page "/"
@using DevInstance.BlazorToolkit.Components
@using DevInstance.BlazorToolkit.Services
@using DevInstance.EmployeeList.Client.Services
@using DevInstance.EmployeeList.Model
@using DevInstance.WebServiceToolkit.Common.Model

@layout BlazorToolkitPageLayout
<PageTitle>Home</PageTitle>

Welcome to your new app.

<ul>
@if (employees != null)
{
    @foreach (var employee in employees.Items)
    {
        <li>@employee.Name</li>
    }
}
</ul>

@code {

    [Inject]
    EmployeeService Service { get; set; }

    [CascadingParameter]
    public IServiceExecutionHost? Host { get; set; }

    private ModelList<EmployeeItem> employees = null;

    protected override async Task OnInitializedAsync()
    {
        await Host.ServiceReadAsync(() => Service.GetItemsAsync(null, null, null), (e) => employees = e);
    }
}
```

See more comprehensive example in `DevInstance.BlazorToolkit.Samples` project.

## License
BlazorToolkit is licensed under the MIT License. You are free to use, modify, and distribute this software in accordance with the terms of the license.

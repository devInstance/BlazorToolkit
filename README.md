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

`Program.cs`
```csharp

static async Task Main(string[] args)
{
    ...
    builder.Services.AddScoped<EmployeeService>();
    ...
}
```

`EmployeeService.cs`
```csharp
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

`Home.razor`
```csharp
@page "/"

<h1>Hello, world!</h1>

Welcome to your new app.

@if (IsError)
{
    <p>Error: @ErrorMessage</p>
}

@if (InProgress)
{
    <p>Loading...</p>
}
else
{
    <ul>
        @if (!IsError && employees != null)
        {
            @foreach (var employee in employees.Items)
            {
                <li>@employee.Name</li>
            }
        }
    </ul>
}

@implements IServiceExecutionHost
@code {

    [Inject]
    EmployeeService Service { get; set; }

    private ModelList<EmployeeItem> employees;

    protected override async Task OnInitializedAsync()
    {
        await this.ServiceCallAsync(() => Service.GetItemsAsync(null, null, null), (e) => employees = e);
    }

    #region IServiceExecutionHost implementation
    public string ErrorMessage { get; set; }

    public bool IsError { get; set; }

    public bool InProgress { get; set; }

    void IServiceExecutionHost.ShowLogin()
    {
        //TODO: navigate to login page
    }

    void IServiceExecutionHost.StateHasChanged()
    {
        StateHasChanged();
    }
    #endregion
}
```

## License
BlazorToolkit is licensed under the MIT License. You are free to use, modify, and distribute this software in accordance with the terms of the license.

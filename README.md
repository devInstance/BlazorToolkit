# BlazorToolkit
BlazorToolkit is a comprehensive set of tools designed to enhance the development of Blazor applications. It provides a suite of utilities and services that streamline common tasks, allowing developers to focus on building rich, interactive web applications.

## Features
- **Network Communication Tools:** Simplify network operations such as making REST API calls. BlazorToolkit offers easy-to-use methods for handling HTTP requests and responses, reducing boilerplate code and potential errors.

- **Middleware Services:** Implement middleware logic as services to abstract complex processes from the UI layer. This promotes a clean separation of concerns, making your application more modular and maintainable.

- **Form Validators:** Integrate robust form validation mechanisms to ensure data integrity. The toolkit includes flexible validators that can be easily applied to your forms, providing immediate feedback to users and enhancing the overall user experience.

## Installation

## Examples

```csharp

builder.Services.

public class EmplyeeService
{
    IApiContext<EmployeeItem> Api { get; set; }

    public EmplyeeService(IApiContext<EmployeeItem> api)
    {
        Api = api;
    }

    public async Task<ServiceActionResult<EmployeeItem?>> GetAsync(string id)
    {
        return await ServiceUtils.HandleWebApiCallAsync(
            async (l) =>
            {
                var api = Api.Get(id);
                return await api.ExecuteAsync();
            }
        );
    }
}
```
## License
BlazorToolkit is licensed under the MIT License. You are free to use, modify, and distribute this software in accordance with the terms of the license.

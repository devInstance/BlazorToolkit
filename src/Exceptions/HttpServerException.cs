using DevInstance.BlazorToolkit.Services;
using System.Net;

namespace DevInstance.BlazorToolkit.Exceptions;

public class HttpServerException : Exception
{
    public HttpServerException(ServiceActionError error, HttpStatusCode statusCode)
    {
        Error = error;
    }
    public HttpStatusCode StatusCode { get; private set; }
    public ServiceActionError Error { get; }
}

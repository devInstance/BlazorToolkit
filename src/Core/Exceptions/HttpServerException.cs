using DevInstance.BlazorToolkit.Services;
using System.Net;

namespace DevInstance.BlazorToolkit.Exceptions;

/// <summary>
/// Represents an exception that occurs during HTTP server communication.
/// </summary>
public class HttpServerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServerException"/> class.
    /// </summary>
    /// <param name="error">The service action error associated with this exception.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    public HttpServerException(ServiceActionError error, HttpStatusCode statusCode)
    {
        Error = error;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the server.
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <summary>
    /// Gets the service action error associated with this exception.
    /// </summary>
    public ServiceActionError Error { get; }
}

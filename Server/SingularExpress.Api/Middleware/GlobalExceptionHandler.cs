using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs;
using SingularExpress.Api.Exceptions;

namespace SingularExpress.Api.Middleware;

/// <summary>
/// Turns every exception into the standard error body <c>{ "errors": [...], "traceId": "..." }</c>.
/// <list type="bullet">
/// <item><see cref="AppException"/> (expected): its status code and message are returned.</item>
/// <item>Anything else (a bug): 500 with a generic message. Details go to the log only,
/// never to the Client, so stack traces and data never leak.</item>
/// </list>
/// Registered in Program.cs with <c>builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c>.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger;

  public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
  {
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken)
  {
    string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
    int statusCode;
    string message;

    if (exception is AppException appException)
    {
      statusCode = appException.StatusCode;
      message = appException.Message;
      _logger.LogInformation(
        "Request failed with {StatusCode}: {Message} (trace {TraceId})",
        statusCode,
        message,
        traceId);
    }
    else
    {
      statusCode = StatusCodes.Status500InternalServerError;
      message = GeneralMessages.UnexpectedError;
      _logger.LogError(exception, "Unhandled exception (trace {TraceId})", traceId);
    }

    httpContext.Response.StatusCode = statusCode;
    await httpContext.Response.WriteAsJsonAsync(
      new ErrorResponseDto { Errors = [message], TraceId = traceId },
      cancellationToken);

    return true;
  }
}

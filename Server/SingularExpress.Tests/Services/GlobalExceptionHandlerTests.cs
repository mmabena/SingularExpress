using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Middleware;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="GlobalExceptionHandler"/>: every error becomes { errors: [...] }.</summary>
public class GlobalExceptionHandlerTests
{
  private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

  [Fact]
  public async Task TryHandleAsync_WithAppException_UsesItsStatusAndMessage()
  {
    (int status, string[] errors) = await Handle(new ConflictException(FinancialPeriodMessages.NoOpenPeriod));

    Assert.Equal(409, status);
    Assert.Equal(new[] { FinancialPeriodMessages.NoOpenPeriod }, errors);
  }

  [Fact]
  public async Task TryHandleAsync_WithUnexpectedException_Returns500WithoutLeakingDetails()
  {
    (int status, string[] errors) = await Handle(new InvalidOperationException("SQL connection string xyz"));

    Assert.Equal(500, status);
    Assert.Equal(new[] { GeneralMessages.UnexpectedError }, errors);
  }

  private async Task<(int Status, string[] Errors)> Handle(Exception exception)
  {
    DefaultHttpContext context = new();
    context.Response.Body = new MemoryStream();

    bool handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
    Assert.True(handled);

    context.Response.Body.Position = 0;
    using JsonDocument json = await JsonDocument.ParseAsync(context.Response.Body);
    string[] errors = json.RootElement.GetProperty("errors")
      .EnumerateArray()
      .Select(e => e.GetString() ?? string.Empty)
      .ToArray();

    return (context.Response.StatusCode, errors);
  }
}

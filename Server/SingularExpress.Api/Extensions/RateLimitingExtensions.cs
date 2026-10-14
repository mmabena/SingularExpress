using System.Threading.RateLimiting;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs;
using SingularExpress.Api.Options;

namespace SingularExpress.Api.Extensions;

/// <summary>
/// Limits how often one IP address can call the auth endpoints, which slows down password
/// and PIN guessing. HRConnect only rate-limited its TOTP endpoint; here all of /api/auth is covered.
/// </summary>
public static class RateLimitingExtensions
{
  public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
  {
    int permitLimit = configuration.GetValue(
      $"{AuthSecurityOptions.SectionName}:{nameof(AuthSecurityOptions.AuthRequestsPerMinute)}",
      5);

    services.AddRateLimiter(options =>
    {
      options.AddPolicy(Policies.AuthRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
          partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
          factory: _ => new FixedWindowRateLimiterOptions
          {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
          }));

      options.OnRejected = async (context, cancellationToken) =>
      {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
          new ErrorResponseDto { Errors = [GeneralMessages.TooManyRequests] },
          cancellationToken);
      };
    });

    return services;
  }
}

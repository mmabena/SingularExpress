using Microsoft.AspNetCore.Mvc;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs;

namespace SingularExpress.Api.Extensions;

/// <summary>
/// Makes DTO validation failures ([Required], [EmailAddress] ...) return the standard
/// <c>{ "errors": [...] }</c> body instead of ASP.NET's default ProblemDetails, so the
/// Client handles every error the same way.
/// </summary>
public static class ApiBehaviourExtensions
{
  public static IMvcBuilder UseStandardValidationResponse(this IMvcBuilder builder)
  {
    return builder.ConfigureApiBehaviorOptions(options =>
    {
      options.InvalidModelStateResponseFactory = context =>
      {
        List<string> errors = context.ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? GeneralMessages.ValidationFailed : e.ErrorMessage)
          .Distinct()
          .ToList();

        return new BadRequestObjectResult(new ErrorResponseDto
        {
          Errors = errors,
          TraceId = context.HttpContext.TraceIdentifier,
        });
      };
    });
  }
}

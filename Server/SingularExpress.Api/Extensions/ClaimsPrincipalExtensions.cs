using System.Globalization;
using System.Security.Claims;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.Exceptions;

namespace SingularExpress.Api.Extensions;

/// <summary>Read values from the signed-in user's JWT inside a controller.</summary>
public static class ClaimsPrincipalExtensions
{
  /// <summary>Usage in a controller: <c>int userId = User.GetUserId();</c></summary>
  public static int GetUserId(this ClaimsPrincipal principal)
  {
    string? value = principal.FindFirstValue(ClaimNames.UserId);
    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int userId))
    {
      return userId;
    }

    throw new UnauthorizedException(GeneralMessages.Unauthenticated);
  }
}

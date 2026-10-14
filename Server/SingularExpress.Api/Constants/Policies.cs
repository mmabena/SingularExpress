namespace SingularExpress.Api.Constants;

/// <summary>
/// Authorization policy names (same three policies as HRConnect).
/// Use them on controllers like this: <c>[Authorize(Policy = Policies.SuperUserOnly)]</c>.
/// They are registered in <see cref="Extensions.AuthenticationExtensions"/>.
/// </summary>
public static class Policies
{
  public const string SuperUserOnly = "SuperUserOnly";
  public const string NormalUserOnly = "NormalUserOnly";
  public const string SuperOrNormalUser = "SuperOrNormalUser";

  /// <summary>Rate-limiting policy applied to every /api/auth endpoint.</summary>
  public const string AuthRateLimit = "auth";
}

namespace SingularExpress.Api.Constants;

/// <summary>
/// Role names as they appear in the JWT "role" claim and in [Authorize(Roles = ...)].
/// Keep these in sync with <see cref="Models.UserRole"/> and Client/src/constants/roles.js.
/// </summary>
public static class Roles
{
  public const string SuperUser = "SuperUser";
  public const string NormalUser = "NormalUser";
}

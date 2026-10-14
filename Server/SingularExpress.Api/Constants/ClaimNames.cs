namespace SingularExpress.Api.Constants;

/// <summary>
/// Claim names written into the JWT. Short, standard names are used on purpose
/// so the Client can read them with jwt-decode (e.g. <c>decoded.role</c>).
/// </summary>
public static class ClaimNames
{
  public const string UserId = "sub";
  public const string Email = "email";
  public const string Role = "role";
}

namespace SingularExpress.Api.DTOs.Auth;

/// <summary>Returned after a successful login. Same shape as HRConnect: { token, user }.</summary>
public class LoginResponseDto
{
  public string Token { get; set; } = string.Empty;

  public DateTime ExpiresAtUtc { get; set; }

  public UserSummaryDto User { get; set; } = new();
}

/// <summary>The minimum the Client needs about the signed-in user.</summary>
public class UserSummaryDto
{
  public int Id { get; set; }

  public string Email { get; set; } = string.Empty;

  /// <summary>"SuperUser" or "NormalUser".</summary>
  public string Role { get; set; } = string.Empty;
}

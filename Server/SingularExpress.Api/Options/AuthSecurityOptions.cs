using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Api.Options;

/// <summary>
/// Login and password-reset security rules, bound from "AuthSecurity".
/// Defaults match HRConnect: 3 failed attempts, 60-second lockout, then a password reset,
/// and a reset PIN valid for 3 minutes.
/// </summary>
public class AuthSecurityOptions
{
  public const string SectionName = "AuthSecurity";

  /// <summary>Fixed so the Client, DTO validation and messages always agree.</summary>
  public const int ResetPinLength = 6;

  [Required]
  public string AllowedEmailDomain { get; set; } = "@singular.co.za";

  [Range(1, 10)]
  public int MaxFailedLoginAttempts { get; set; } = 3;

  [Range(10, 3600)]
  public int LockoutSeconds { get; set; } = 60;

  [Range(1, 30)]
  public int ResetPinExpiryMinutes { get; set; } = 3;

  /// <summary>Wrong PIN guesses allowed before the PIN is cancelled.</summary>
  [Range(1, 10)]
  public int MaxResetPinAttempts { get; set; } = 5;

  /// <summary>How many previous passwords a user may not reuse (current one is always blocked).</summary>
  [Range(0, 24)]
  public int PasswordHistoryCount { get; set; } = 5;

  /// <summary>Requests per minute per IP address on /api/auth endpoints.</summary>
  [Range(1, 100)]
  public int AuthRequestsPerMinute { get; set; } = 5;
}

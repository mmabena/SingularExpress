namespace SingularExpress.Api.Models;

/// <summary>
/// A person who can sign in. Only the data the login flow needs is stored here (POPIA/GDPR
/// data minimisation). Lockout state is stored in the database, not in memory, so it
/// survives restarts and works when more than one API instance runs.
/// </summary>
public class User
{
  public int Id { get; set; }

  /// <summary>Always stored in lower case. Must end with the allowed domain.</summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>ASP.NET Core Identity PBKDF2 hash. The plain password is never stored or logged.</summary>
  public string PasswordHash { get; set; } = string.Empty;

  public UserRole Role { get; set; } = UserRole.NormalUser;

  public bool IsActive { get; set; } = true;

  /// <summary>Consecutive wrong passwords since the last successful login or lockout.</summary>
  public int FailedLoginAttempts { get; set; }

  /// <summary>While this is in the future, login is refused.</summary>
  public DateTime? LockoutEndUtc { get; set; }

  /// <summary>Set when the account locks. Cleared only by a successful password reset.</summary>
  public bool RequiresPasswordReset { get; set; }

  public DateTime CreatedAtUtc { get; set; }
}

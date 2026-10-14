namespace SingularExpress.Api.Models;

/// <summary>
/// A one-time PIN emailed to a user who forgot their password.
/// Only a hash of the PIN is stored, so a database leak does not reveal live PINs.
/// </summary>
public class PasswordResetPin
{
  public int Id { get; set; }

  public int UserId { get; set; }

  public string PinHash { get; set; } = string.Empty;

  public DateTime ExpiresAtUtc { get; set; }

  public bool IsUsed { get; set; }

  /// <summary>Wrong guesses for this PIN. When it reaches the limit the PIN is cancelled.</summary>
  public int FailedAttempts { get; set; }

  public DateTime CreatedAtUtc { get; set; }

  public User? User { get; set; }
}

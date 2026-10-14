namespace SingularExpress.Api.Models;

/// <summary>Previous password hashes, used to stop users reusing old passwords.</summary>
public class PasswordHistory
{
  public int Id { get; set; }

  public int UserId { get; set; }

  public string PasswordHash { get; set; } = string.Empty;

  public DateTime ChangedAtUtc { get; set; }

  public User? User { get; set; }
}

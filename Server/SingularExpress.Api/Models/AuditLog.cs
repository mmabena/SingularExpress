namespace SingularExpress.Api.Models;

/// <summary>
/// Record of an important, irreversible action (for example a period rollover).
/// Store user ids, not names or emails, to keep personal information out of the log.
/// </summary>
public class AuditLog
{
  public long Id { get; set; }

  /// <summary>One of <see cref="Constants.AuditActions"/>.</summary>
  public string Action { get; set; } = string.Empty;

  public string EntityName { get; set; } = string.Empty;

  public string EntityId { get; set; } = string.Empty;

  /// <summary>Null when the system performed the action (e.g. first-run setup).</summary>
  public int? PerformedByUserId { get; set; }

  public string Details { get; set; } = string.Empty;

  public DateTime CreatedAtUtc { get; set; }
}

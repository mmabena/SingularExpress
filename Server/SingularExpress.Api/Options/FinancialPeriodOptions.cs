using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Api.Options;

/// <summary>Financial period settings, bound from "FinancialPeriod".</summary>
public class FinancialPeriodOptions
{
  public const string SectionName = "FinancialPeriod";

  /// <summary>
  /// The first period opened on a brand-new database, as yyyy-MM. Defaults to October 2026
  /// (period 7 of FY2026/27). Ignored once any period exists.
  /// </summary>
  [Required]
  [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$")]
  public string InitialPeriod { get; set; } = "2026-10";

  /// <summary>Time zone used to decide "today" when checking whether a period has ended.</summary>
  [Required]
  public string TimeZoneId { get; set; } = "Africa/Johannesburg";

  /// <summary>
  /// false (production default): rollover is refused until the open period's last day has passed.
  /// true (development/training): a SuperUser can roll over at any time.
  /// </summary>
  public bool AllowRolloverBeforePeriodEnd { get; set; }
}

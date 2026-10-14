using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Api.Models;

/// <summary>
/// One calendar month inside a financial year that runs April to March.
/// <para>
/// Example: October 2026 is <c>FinancialYearStart = 2026</c>, <c>PeriodNumber = 7</c>,
/// shown as "2026/27 P07". See docs/FINANCIAL_PERIODS.md.
/// </para>
/// </summary>
public class FinancialPeriod
{
  public int Id { get; set; }

  /// <summary>Calendar year in which the financial year starts (April). FY2026/27 → 2026.</summary>
  public int FinancialYearStart { get; set; }

  /// <summary>1 = April … 12 = March.</summary>
  public int PeriodNumber { get; set; }

  public int CalendarYear { get; set; }

  /// <summary>1 = January … 12 = December.</summary>
  public int CalendarMonth { get; set; }

  public DateOnly StartDate { get; set; }

  public DateOnly EndDate { get; set; }

  public PeriodStatus Status { get; set; }

  public DateTime OpenedAtUtc { get; set; }

  public DateTime? ClosedAtUtc { get; set; }

  public int? ClosedByUserId { get; set; }

  /// <summary>
  /// Concurrency token. If two SuperUsers roll over at the same moment, the second save
  /// fails instead of silently skipping a month.
  /// </summary>
  [Timestamp]
  public byte[] RowVersion { get; set; } = [];
}

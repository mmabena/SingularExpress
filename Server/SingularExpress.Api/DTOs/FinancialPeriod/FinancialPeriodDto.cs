namespace SingularExpress.Api.DTOs.FinancialPeriod;

/// <summary>A stored financial period as the Client sees it.</summary>
public class FinancialPeriodDto
{
  public int Id { get; set; }

  /// <summary>e.g. "2026/27".</summary>
  public string FinancialYear { get; set; } = string.Empty;

  public int FinancialYearStart { get; set; }

  public int PeriodNumber { get; set; }

  /// <summary>e.g. "2026/27 P07 (October 2026)".</summary>
  public string Label { get; set; } = string.Empty;

  public int CalendarYear { get; set; }

  public int CalendarMonth { get; set; }

  public string MonthName { get; set; } = string.Empty;

  public DateOnly StartDate { get; set; }

  public DateOnly EndDate { get; set; }

  /// <summary>"Open" or "Closed".</summary>
  public string Status { get; set; } = string.Empty;

  public DateTime? ClosedAtUtc { get; set; }
}

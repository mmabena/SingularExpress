namespace SingularExpress.Api.DTOs.FinancialPeriod;

/// <summary>All 12 months of one financial year, including months not opened yet.</summary>
public class FinancialYearOverviewDto
{
  public string FinancialYear { get; set; } = string.Empty;

  public int FinancialYearStart { get; set; }

  public IReadOnlyList<PeriodSlotDto> Periods { get; set; } = [];
}

/// <summary>One month in <see cref="FinancialYearOverviewDto"/>.</summary>
public class PeriodSlotDto
{
  public int PeriodNumber { get; set; }

  public int CalendarYear { get; set; }

  public int CalendarMonth { get; set; }

  public string MonthName { get; set; } = string.Empty;

  /// <summary>"Closed", "Open" or "Upcoming" (not created yet).</summary>
  public string Status { get; set; } = string.Empty;
}

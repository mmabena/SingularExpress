namespace SingularExpress.Api.DTOs.FinancialPeriod;

/// <summary>Result of a successful rollover.</summary>
public class RolloverResultDto
{
  public FinancialPeriodDto ClosedPeriod { get; set; } = new();

  public FinancialPeriodDto OpenedPeriod { get; set; } = new();

  /// <summary>True when March was closed and April of a new financial year opened.</summary>
  public bool StartedNewFinancialYear { get; set; }

  public string Message { get; set; } = string.Empty;
}

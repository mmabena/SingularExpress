using SingularExpress.Api.DTOs.FinancialPeriod;

namespace SingularExpress.Api.Interfaces;

/// <summary>Financial periods (April-March) and the SuperUser-only month-end rollover.</summary>
public interface IFinancialPeriodService
{
  Task<FinancialPeriodDto> GetCurrentPeriodAsync();

  Task<FinancialYearOverviewDto> GetFinancialYearOverviewAsync(int financialYearStart);

  /// <summary>
  /// Closes the open period and opens the next one (March rolls into April of a new year).
  /// The caller's role is checked here as well as on the controller.
  /// </summary>
  Task<RolloverResultDto> RolloverAsync(RolloverRequestDto request, int performedByUserId);

  /// <summary>Creates the first open period on an empty database. Safe to call on every start.</summary>
  Task EnsureInitialPeriodAsync();
}

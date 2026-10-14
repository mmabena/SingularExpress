using SingularExpress.Api.Models;

namespace SingularExpress.Api.Interfaces;

/// <summary>Database access for <see cref="FinancialPeriod"/> and its audit trail.</summary>
public interface IFinancialPeriodRepository
{
  Task<FinancialPeriod?> GetOpenPeriodAsync();

  Task<bool> AnyAsync();

  Task<bool> ExistsAsync(int financialYearStart, int periodNumber);

  Task<IReadOnlyList<FinancialPeriod>> GetByFinancialYearAsync(int financialYearStart);

  /// <summary>Creates the very first period together with its audit entry.</summary>
  Task AddInitialPeriodAsync(FinancialPeriod period, AuditLog audit);

  /// <summary>
  /// Saves the closed period, the new open period and the audit entry in ONE transaction:
  /// either all three are stored or none are. Throws <see cref="Exceptions.ConflictException"/>
  /// if another rollover was saved first.
  /// </summary>
  Task CommitRolloverAsync(FinancialPeriod closedPeriod, FinancialPeriod openedPeriod, AuditLog audit);
}

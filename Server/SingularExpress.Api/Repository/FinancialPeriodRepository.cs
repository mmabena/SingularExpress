using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.Data;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;

namespace SingularExpress.Api.Repository;

/// <inheritdoc cref="IFinancialPeriodRepository"/>
public class FinancialPeriodRepository : IFinancialPeriodRepository
{
  private readonly ApplicationDbContext _context;

  public FinancialPeriodRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public Task<FinancialPeriod?> GetOpenPeriodAsync()
  {
    return _context.FinancialPeriods.FirstOrDefaultAsync(p => p.Status == PeriodStatus.Open);
  }

  public Task<bool> AnyAsync()
  {
    return _context.FinancialPeriods.AnyAsync();
  }

  public Task<bool> ExistsAsync(int financialYearStart, int periodNumber)
  {
    return _context.FinancialPeriods.AnyAsync(p =>
      p.FinancialYearStart == financialYearStart && p.PeriodNumber == periodNumber);
  }

  public async Task<IReadOnlyList<FinancialPeriod>> GetByFinancialYearAsync(int financialYearStart)
  {
    return await _context.FinancialPeriods
      .AsNoTracking()
      .Where(p => p.FinancialYearStart == financialYearStart)
      .OrderBy(p => p.PeriodNumber)
      .ToListAsync();
  }

  public async Task AddInitialPeriodAsync(FinancialPeriod period, AuditLog audit)
  {
    await _context.FinancialPeriods.AddAsync(period);
    await _context.AuditLogs.AddAsync(audit);
    await _context.SaveChangesAsync();
  }

  public async Task CommitRolloverAsync(FinancialPeriod closedPeriod, FinancialPeriod openedPeriod, AuditLog audit)
  {
    await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Save the close first so the "single open period" index is never violated.
      _context.FinancialPeriods.Update(closedPeriod);
      await _context.SaveChangesAsync();

      await _context.FinancialPeriods.AddAsync(openedPeriod);
      await _context.AuditLogs.AddAsync(audit);
      await _context.SaveChangesAsync();

      await transaction.CommitAsync();
    }
    catch (DbUpdateException)
    {
      // DbUpdateConcurrencyException (RowVersion changed) or a unique index clash:
      // someone else rolled over first. Nothing was saved.
      await transaction.RollbackAsync();
      throw new ConflictException(FinancialPeriodMessages.ConcurrentRollover);
    }
  }
}

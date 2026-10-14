using Microsoft.EntityFrameworkCore;
using SingularExpress.Api.Data;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;

namespace SingularExpress.Api.Repository;

/// <inheritdoc cref="IPasswordResetRepository"/>
public class PasswordResetRepository : IPasswordResetRepository
{
  private readonly ApplicationDbContext _context;

  public PasswordResetRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task AddPinAsync(PasswordResetPin pin)
  {
    await _context.PasswordResetPins.AddAsync(pin);
    await _context.SaveChangesAsync();
  }

  public Task<PasswordResetPin?> GetLatestActivePinAsync(int userId, DateTime nowUtc)
  {
    return _context.PasswordResetPins
      .Where(p => p.UserId == userId && !p.IsUsed && p.ExpiresAtUtc > nowUtc)
      .OrderByDescending(p => p.CreatedAtUtc)
      .FirstOrDefaultAsync();
  }

  public async Task InvalidateActivePinsAsync(int userId)
  {
    List<PasswordResetPin> active = await _context.PasswordResetPins
      .Where(p => p.UserId == userId && !p.IsUsed)
      .ToListAsync();

    foreach (PasswordResetPin pin in active)
    {
      pin.IsUsed = true;
    }

    await _context.SaveChangesAsync();
  }

  public async Task UpdatePinAsync(PasswordResetPin pin)
  {
    _context.PasswordResetPins.Update(pin);
    await _context.SaveChangesAsync();
  }

  public async Task AddPasswordHistoryAsync(PasswordHistory history)
  {
    await _context.PasswordHistories.AddAsync(history);
    await _context.SaveChangesAsync();
  }

  public async Task<IReadOnlyList<string>> GetRecentPasswordHashesAsync(int userId, int count)
  {
    if (count <= 0)
    {
      return [];
    }

    return await _context.PasswordHistories
      .Where(h => h.UserId == userId)
      .OrderByDescending(h => h.ChangedAtUtc)
      .Take(count)
      .Select(h => h.PasswordHash)
      .ToListAsync();
  }
}

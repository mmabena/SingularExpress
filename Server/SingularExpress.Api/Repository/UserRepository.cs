using Microsoft.EntityFrameworkCore;
using SingularExpress.Api.Data;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;

namespace SingularExpress.Api.Repository;

/// <inheritdoc cref="IUserRepository"/>
public class UserRepository : IUserRepository
{
  private readonly ApplicationDbContext _context;

  public UserRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public Task<User?> GetByEmailAsync(string normalisedEmail)
  {
    return _context.Users.FirstOrDefaultAsync(u => u.Email == normalisedEmail);
  }

  public Task<User?> GetByIdAsync(int userId)
  {
    return _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
  }

  public Task<bool> AnyWithRoleAsync(UserRole role)
  {
    return _context.Users.AnyAsync(u => u.Role == role);
  }

  public async Task AddAsync(User user)
  {
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(User user)
  {
    _context.Users.Update(user);
    await _context.SaveChangesAsync();
  }
}

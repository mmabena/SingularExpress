using SingularExpress.Api.Models;

namespace SingularExpress.Api.Interfaces;

/// <summary>Database access for <see cref="User"/>. No business rules here: those live in services.</summary>
public interface IUserRepository
{
  /// <summary>Looks up by normalised (lower-case) email. Returns null when not found.</summary>
  Task<User?> GetByEmailAsync(string normalisedEmail);

  Task<User?> GetByIdAsync(int userId);

  Task<bool> AnyWithRoleAsync(UserRole role);

  Task AddAsync(User user);

  /// <summary>Saves changes made to a user that was loaded from this repository.</summary>
  Task UpdateAsync(User user);
}

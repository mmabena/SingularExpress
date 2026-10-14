using SingularExpress.Api.Models;

namespace SingularExpress.Api.Interfaces;

/// <summary>Database access for reset PINs and password history.</summary>
public interface IPasswordResetRepository
{
  Task AddPinAsync(PasswordResetPin pin);

  /// <summary>The newest PIN for this user that is unused and not expired at <paramref name="nowUtc"/>.</summary>
  Task<PasswordResetPin?> GetLatestActivePinAsync(int userId, DateTime nowUtc);

  /// <summary>Marks every unused PIN for the user as used, so only the newest PIN works.</summary>
  Task InvalidateActivePinsAsync(int userId);

  Task UpdatePinAsync(PasswordResetPin pin);

  Task AddPasswordHistoryAsync(PasswordHistory history);

  /// <summary>Most recent hashes first, at most <paramref name="count"/>.</summary>
  Task<IReadOnlyList<string>> GetRecentPasswordHashesAsync(int userId, int count);
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Data;

/// <summary>
/// Runs once at start-up (see Program.cs).
/// <list type="number">
/// <item>Always: makes sure an open financial period exists (October 2026 on a new database).</item>
/// <item>Development only: creates a first SuperUser if Seed:SuperUserEmail and
/// Seed:SuperUserPassword are set with user-secrets and no SuperUser exists yet.</item>
/// </list>
/// </summary>
public class DbSeeder
{
  private readonly IFinancialPeriodService _periodService;
  private readonly IUserRepository _userRepository;
  private readonly IPasswordHasher<User> _passwordHasher;
  private readonly IHostEnvironment _environment;
  private readonly SeedOptions _seedOptions;
  private readonly AuthSecurityOptions _authOptions;
  private readonly TimeProvider _timeProvider;
  private readonly ILogger<DbSeeder> _logger;

  public DbSeeder(
    IFinancialPeriodService periodService,
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IHostEnvironment environment,
    IOptions<SeedOptions> seedOptions,
    IOptions<AuthSecurityOptions> authOptions,
    TimeProvider timeProvider,
    ILogger<DbSeeder> logger)
  {
    _periodService = periodService;
    _userRepository = userRepository;
    _passwordHasher = passwordHasher;
    _environment = environment;
    _seedOptions = seedOptions.Value;
    _authOptions = authOptions.Value;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  public async Task SeedAsync()
  {
    await _periodService.EnsureInitialPeriodAsync();

    if (_environment.IsDevelopment())
    {
      await SeedDevelopmentSuperUserAsync();
    }
  }

  private async Task SeedDevelopmentSuperUserAsync()
  {
    if (string.IsNullOrWhiteSpace(_seedOptions.SuperUserEmail)
      || string.IsNullOrWhiteSpace(_seedOptions.SuperUserPassword))
    {
      return;
    }

    if (await _userRepository.AnyWithRoleAsync(UserRole.SuperUser))
    {
      return;
    }

    string email = EmailHelper.Normalise(_seedOptions.SuperUserEmail);
    if (!EmailHelper.HasAllowedDomain(email, _authOptions.AllowedEmailDomain)
      || !PasswordValidator.IsValidPassword(_seedOptions.SuperUserPassword))
    {
      _logger.LogWarning("Seed SuperUser skipped: email domain or password complexity is invalid");
      return;
    }

    User user = new()
    {
      Email = email,
      Role = UserRole.SuperUser,
      IsActive = true,
      CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
    };
    user.PasswordHash = _passwordHasher.HashPassword(user, _seedOptions.SuperUserPassword);

    await _userRepository.AddAsync(user);
    _logger.LogInformation("Development SuperUser {Email} created", EmailHelper.Mask(email));
  }
}

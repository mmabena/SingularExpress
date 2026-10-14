using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs.Auth;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Services;

/// <summary>
/// Login and forgot-password flow, following HRConnect's rules with its security gaps closed.
/// <list type="bullet">
/// <item>Only @singular.co.za addresses may sign in.</item>
/// <item>3 wrong passwords lock the account for 60 seconds; after that a password reset is required.</item>
/// <item>Reset PIN: emailed, valid for 3 minutes, single use, stored only as a hash.</item>
/// <item>New passwords must be complex and must not match the current or recent passwords.</item>
/// </list>
/// All user-facing text comes from <see cref="AuthMessages"/>. See docs/AUTHENTICATION.md.
/// </summary>
public class AuthService : IAuthService
{
  /// <summary>
  /// Used when the email has no account: we still run a hash check so the response takes
  /// about as long as a real one, and response time cannot reveal which emails exist.
  /// </summary>
  private static readonly string DummyPasswordHash =
    new PasswordHasher<User>().HashPassword(new User(), "Dummy-Password-For-Timing-1!");

  private readonly IUserRepository _userRepository;
  private readonly IPasswordResetRepository _passwordResetRepository;
  private readonly IPasswordHasher<User> _passwordHasher;
  private readonly IPasswordHasher<PasswordResetPin> _pinHasher;
  private readonly ITokenService _tokenService;
  private readonly IEmailService _emailService;
  private readonly IPinGenerator _pinGenerator;
  private readonly TimeProvider _timeProvider;
  private readonly AuthSecurityOptions _options;
  private readonly ILogger<AuthService> _logger;

  public AuthService(
    IUserRepository userRepository,
    IPasswordResetRepository passwordResetRepository,
    IPasswordHasher<User> passwordHasher,
    IPasswordHasher<PasswordResetPin> pinHasher,
    ITokenService tokenService,
    IEmailService emailService,
    IPinGenerator pinGenerator,
    TimeProvider timeProvider,
    IOptions<AuthSecurityOptions> options,
    ILogger<AuthService> logger)
  {
    _userRepository = userRepository;
    _passwordResetRepository = passwordResetRepository;
    _passwordHasher = passwordHasher;
    _pinHasher = pinHasher;
    _tokenService = tokenService;
    _emailService = emailService;
    _pinGenerator = pinGenerator;
    _timeProvider = timeProvider;
    _options = options.Value;
    _logger = logger;
  }

  public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
  {
    string email = EnsureAllowedEmail(request.Email);
    DateTime nowUtc = UtcNow();

    User? user = await _userRepository.GetByEmailAsync(email);
    if (user is null || !user.IsActive)
    {
      _passwordHasher.VerifyHashedPassword(new User(), DummyPasswordHash, request.Password);
      throw new UnauthorizedException(AuthMessages.InvalidCredentials);
    }

    if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > nowUtc)
    {
      throw new AccountLockedException(AuthMessages.AccountTemporarilyLocked);
    }

    if (user.RequiresPasswordReset)
    {
      throw new AccountLockedException(AuthMessages.PasswordResetRequired);
    }

    PasswordVerificationResult result =
      _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

    if (result == PasswordVerificationResult.Failed)
    {
      await RecordFailedLoginAsync(user, nowUtc);
      // RecordFailedLoginAsync throws when the account locks; otherwise:
      throw new UnauthorizedException(AuthMessages.InvalidCredentials);
    }

    if (result == PasswordVerificationResult.SuccessRehashNeeded)
    {
      user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
    }

    user.FailedLoginAttempts = 0;
    user.LockoutEndUtc = null;
    await _userRepository.UpdateAsync(user);

    (string token, DateTime expiresAtUtc) = _tokenService.CreateToken(user);

    return new LoginResponseDto
    {
      Token = token,
      ExpiresAtUtc = expiresAtUtc,
      User = new UserSummaryDto
      {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role.ToString(),
      },
    };
  }

  public async Task ForgotPasswordAsync(ForgotPasswordDto request)
  {
    string email = EnsureAllowedEmail(request.Email);

    User? user = await _userRepository.GetByEmailAsync(email);
    if (user is null || !user.IsActive)
    {
      // Same outcome as a real account: the controller always returns the same message.
      _logger.LogInformation("Password reset requested for unknown address {Email}", EmailHelper.Mask(email));
      return;
    }

    // Only the newest PIN should work.
    await _passwordResetRepository.InvalidateActivePinsAsync(user.Id);

    DateTime nowUtc = UtcNow();
    string pin = _pinGenerator.Generate(AuthSecurityOptions.ResetPinLength);

    PasswordResetPin resetPin = new()
    {
      UserId = user.Id,
      ExpiresAtUtc = nowUtc.AddMinutes(_options.ResetPinExpiryMinutes),
      CreatedAtUtc = nowUtc,
    };
    resetPin.PinHash = _pinHasher.HashPassword(resetPin, pin);
    await _passwordResetRepository.AddPinAsync(resetPin);

    string body = string.Format(
      CultureInfo.InvariantCulture,
      EmailTemplates.PasswordResetBody,
      pin,
      _options.ResetPinExpiryMinutes);
    await _emailService.SendAsync(user.Email, EmailTemplates.PasswordResetSubject, body);
  }

  public async Task VerifyPinAsync(VerifyPinDto request)
  {
    string email = EnsureAllowedEmail(request.Email);
    await GetUserWithValidPinAsync(email, request.Pin);
  }

  public async Task ResetPasswordAsync(ResetPasswordDto request)
  {
    string email = EnsureAllowedEmail(request.Email);

    if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
    {
      throw new BadRequestException(AuthMessages.PasswordsDoNotMatch);
    }

    if (!PasswordValidator.IsValidPassword(request.NewPassword))
    {
      throw new BadRequestException(AuthMessages.PasswordComplexity);
    }

    (User user, PasswordResetPin pin) = await GetUserWithValidPinAsync(email, request.Pin);

    if (await IsPasswordReusedAsync(user, request.NewPassword))
    {
      throw new BadRequestException(AuthMessages.PasswordPreviouslyUsed);
    }

    DateTime nowUtc = UtcNow();

    await _passwordResetRepository.AddPasswordHistoryAsync(new PasswordHistory
    {
      UserId = user.Id,
      PasswordHash = user.PasswordHash,
      ChangedAtUtc = nowUtc,
    });

    user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
    user.FailedLoginAttempts = 0;
    user.LockoutEndUtc = null;
    user.RequiresPasswordReset = false;
    await _userRepository.UpdateAsync(user);

    pin.IsUsed = true;
    await _passwordResetRepository.UpdatePinAsync(pin);
  }

  /// <summary>Counts a wrong password and locks the account when the limit is reached.</summary>
  private async Task RecordFailedLoginAsync(User user, DateTime nowUtc)
  {
    user.FailedLoginAttempts++;

    if (user.FailedLoginAttempts >= _options.MaxFailedLoginAttempts)
    {
      user.FailedLoginAttempts = 0;
      user.LockoutEndUtc = nowUtc.AddSeconds(_options.LockoutSeconds);
      user.RequiresPasswordReset = true;
      await _userRepository.UpdateAsync(user);

      _logger.LogWarning("User {UserId} locked after too many failed logins", user.Id);
      throw new AccountLockedException(string.Format(
        CultureInfo.InvariantCulture,
        AuthMessages.AccountLockedTooManyAttempts,
        _options.LockoutSeconds));
    }

    await _userRepository.UpdateAsync(user);
  }

  /// <summary>
  /// Shared by verify-pin and reset-password. Every failure gives the same message so the
  /// endpoint cannot be used to discover accounts. Wrong guesses are counted per PIN.
  /// </summary>
  private async Task<(User User, PasswordResetPin Pin)> GetUserWithValidPinAsync(string email, string pin)
  {
    User? user = await _userRepository.GetByEmailAsync(email);
    if (user is null || !user.IsActive)
    {
      throw new BadRequestException(AuthMessages.InvalidOrExpiredPin);
    }

    PasswordResetPin? resetPin =
      await _passwordResetRepository.GetLatestActivePinAsync(user.Id, UtcNow());
    if (resetPin is null)
    {
      throw new BadRequestException(AuthMessages.InvalidOrExpiredPin);
    }

    PasswordVerificationResult result = _pinHasher.VerifyHashedPassword(resetPin, resetPin.PinHash, pin);
    if (result == PasswordVerificationResult.Failed)
    {
      resetPin.FailedAttempts++;
      if (resetPin.FailedAttempts >= _options.MaxResetPinAttempts)
      {
        resetPin.IsUsed = true; // cancel the PIN: the user must request a new one
      }

      await _passwordResetRepository.UpdatePinAsync(resetPin);
      throw new BadRequestException(AuthMessages.InvalidOrExpiredPin);
    }

    return (user, resetPin);
  }

  /// <summary>
  /// Checks the new password against the current hash and recent history by verifying it,
  /// not by comparing hashes. (HRConnect compared two salted hashes, which never match.)
  /// </summary>
  private async Task<bool> IsPasswordReusedAsync(User user, string newPassword)
  {
    List<string> hashesToCheck = [user.PasswordHash];
    hashesToCheck.AddRange(
      await _passwordResetRepository.GetRecentPasswordHashesAsync(user.Id, _options.PasswordHistoryCount));

    foreach (string hash in hashesToCheck)
    {
      if (string.IsNullOrEmpty(hash))
      {
        continue;
      }

      if (_passwordHasher.VerifyHashedPassword(user, hash, newPassword) != PasswordVerificationResult.Failed)
      {
        return true;
      }
    }

    return false;
  }

  private string EnsureAllowedEmail(string? email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new BadRequestException(AuthMessages.EmailRequired);
    }

    if (!EmailHelper.HasAllowedDomain(email, _options.AllowedEmailDomain))
    {
      throw new BadRequestException(AuthMessages.InvalidEmailDomain);
    }

    return EmailHelper.Normalise(email);
  }

  private DateTime UtcNow()
  {
    return _timeProvider.GetUtcNow().UtcDateTime;
  }
}

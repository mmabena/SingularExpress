using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs.Auth;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Services;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>
/// Unit tests for <see cref="AuthService"/>.
/// Repositories, email, token and PIN generator are mocked; password hashing is real so
/// the tests prove the actual hashing behaviour.
/// </summary>
public class AuthServiceTests
{
  private const string Pin = "482915";

  private readonly Mock<IUserRepository> _users = new();
  private readonly Mock<IPasswordResetRepository> _resets = new();
  private readonly Mock<ITokenService> _tokens = new();
  private readonly Mock<IEmailService> _email = new();
  private readonly Mock<IPinGenerator> _pinGenerator = new();
  private readonly PasswordHasher<PasswordResetPin> _pinHasher = new();
  private readonly FakeTimeProvider _time = new(TestData.Now);
  private readonly AuthSecurityOptions _options = new();
  private readonly AuthService _service;

  public AuthServiceTests()
  {
    _tokens.Setup(t => t.CreateToken(It.IsAny<User>()))
      .Returns(("jwt-token", TestData.Now.UtcDateTime.AddMinutes(60)));
    _pinGenerator.Setup(g => g.Generate(AuthSecurityOptions.ResetPinLength)).Returns(Pin);
    _resets.Setup(r => r.GetRecentPasswordHashesAsync(It.IsAny<int>(), It.IsAny<int>()))
      .ReturnsAsync(Array.Empty<string>());

    _service = new AuthService(
      _users.Object,
      _resets.Object,
      new PasswordHasher<User>(),
      _pinHasher,
      _tokens.Object,
      _email.Object,
      _pinGenerator.Object,
      _time,
      TestOptions.Of(_options),
      NullLogger<AuthService>.Instance);
  }

  // ---------------------------------------------------------------- Login

  [Fact]
  public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
  {
    User user = GivenUser(UserRole.SuperUser);

    LoginResponseDto result = await _service.LoginAsync(Login(TestData.Email, TestData.Password));

    Assert.Equal("jwt-token", result.Token);
    Assert.Equal(user.Id, result.User.Id);
    Assert.Equal("SuperUser", result.User.Role);
  }

  [Fact]
  public async Task LoginAsync_WithMixedCaseAndSpaces_NormalisesEmail()
  {
    GivenUser();

    await _service.LoginAsync(Login("  Test.User@Singular.CO.ZA ", TestData.Password));

    _users.Verify(u => u.GetByEmailAsync(TestData.Email), Times.Once);
  }

  [Theory]
  [InlineData("someone@gmail.com")]
  [InlineData("someone@singular.co.za.evil.com")]
  [InlineData("@singular.co.za")]
  public async Task LoginAsync_WithEmailOutsideAllowedDomain_ThrowsBadRequest(string email)
  {
    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.LoginAsync(Login(email, TestData.Password)));

    Assert.Equal(AuthMessages.InvalidEmailDomain, ex.Message);
  }

  [Fact]
  public async Task LoginAsync_WithUnknownEmail_ThrowsSameMessageAsWrongPassword()
  {
    _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(
      () => _service.LoginAsync(Login(TestData.Email, TestData.Password)));

    Assert.Equal(AuthMessages.InvalidCredentials, ex.Message);
  }

  [Fact]
  public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorized()
  {
    User user = GivenUser();
    user.IsActive = false;

    await Assert.ThrowsAsync<UnauthorizedException>(
      () => _service.LoginAsync(Login(TestData.Email, TestData.Password)));
  }

  [Fact]
  public async Task LoginAsync_WithWrongPassword_CountsFailedAttempt()
  {
    User user = GivenUser();

    UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(
      () => _service.LoginAsync(Login(TestData.Email, "Wrong-Password-1!")));

    Assert.Equal(AuthMessages.InvalidCredentials, ex.Message);
    Assert.Equal(1, user.FailedLoginAttempts);
    Assert.Null(user.LockoutEndUtc);
    _users.Verify(u => u.UpdateAsync(user), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_OnThirdWrongPassword_LocksAccountFor60SecondsAndRequiresReset()
  {
    User user = GivenUser();
    user.FailedLoginAttempts = 2;

    AccountLockedException ex = await Assert.ThrowsAsync<AccountLockedException>(
      () => _service.LoginAsync(Login(TestData.Email, "Wrong-Password-1!")));

    Assert.Contains("60 seconds", ex.Message, StringComparison.Ordinal);
    Assert.Equal(TestData.Now.UtcDateTime.AddSeconds(60), user.LockoutEndUtc);
    Assert.True(user.RequiresPasswordReset);
  }

  [Fact]
  public async Task LoginAsync_WhileLocked_RefusesEvenTheCorrectPassword()
  {
    User user = GivenUser();
    user.LockoutEndUtc = TestData.Now.UtcDateTime.AddSeconds(30);
    user.RequiresPasswordReset = true;

    AccountLockedException ex = await Assert.ThrowsAsync<AccountLockedException>(
      () => _service.LoginAsync(Login(TestData.Email, TestData.Password)));

    Assert.Equal(AuthMessages.AccountTemporarilyLocked, ex.Message);
    _tokens.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_AfterLockoutExpires_StillRequiresPasswordReset()
  {
    User user = GivenUser();
    user.LockoutEndUtc = TestData.Now.UtcDateTime.AddSeconds(-1);
    user.RequiresPasswordReset = true;

    AccountLockedException ex = await Assert.ThrowsAsync<AccountLockedException>(
      () => _service.LoginAsync(Login(TestData.Email, TestData.Password)));

    Assert.Equal(AuthMessages.PasswordResetRequired, ex.Message);
  }

  [Fact]
  public async Task LoginAsync_OnSuccess_ClearsFailedAttempts()
  {
    User user = GivenUser();
    user.FailedLoginAttempts = 2;

    await _service.LoginAsync(Login(TestData.Email, TestData.Password));

    Assert.Equal(0, user.FailedLoginAttempts);
  }

  // ---------------------------------------------------------------- Forgot password

  [Fact]
  public async Task ForgotPasswordAsync_ForUnknownEmail_SendsNothingAndDoesNotThrow()
  {
    _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    await _service.ForgotPasswordAsync(new ForgotPasswordDto { Email = TestData.Email });

    _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    _resets.Verify(r => r.AddPinAsync(It.IsAny<PasswordResetPin>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPasswordAsync_ForKnownEmail_StoresHashedPinAndEmailsPlainPin()
  {
    User user = GivenUser();
    PasswordResetPin? stored = null;
    _resets.Setup(r => r.AddPinAsync(It.IsAny<PasswordResetPin>()))
      .Callback<PasswordResetPin>(p => stored = p)
      .Returns(Task.CompletedTask);

    await _service.ForgotPasswordAsync(new ForgotPasswordDto { Email = TestData.Email });

    Assert.NotNull(stored);
    Assert.NotEqual(Pin, stored.PinHash);
    Assert.Equal(user.Id, stored.UserId);
    Assert.Equal(TestData.Now.UtcDateTime.AddMinutes(3), stored.ExpiresAtUtc);
    _email.Verify(e => e.SendAsync(
      TestData.Email,
      EmailTemplates.PasswordResetSubject,
      It.Is<string>(body => body.Contains(Pin, StringComparison.Ordinal))), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordAsync_CancelsOlderPinsFirst()
  {
    User user = GivenUser();

    await _service.ForgotPasswordAsync(new ForgotPasswordDto { Email = TestData.Email });

    _resets.Verify(r => r.InvalidateActivePinsAsync(user.Id), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordAsync_WithWrongDomain_ThrowsBadRequest()
  {
    await Assert.ThrowsAsync<BadRequestException>(
      () => _service.ForgotPasswordAsync(new ForgotPasswordDto { Email = "someone@gmail.com" }));
  }

  // ---------------------------------------------------------------- Verify PIN

  [Fact]
  public async Task VerifyPinAsync_WithCorrectPin_Succeeds()
  {
    User user = GivenUser();
    GivenActivePin(user);

    await _service.VerifyPinAsync(new VerifyPinDto { Email = TestData.Email, Pin = Pin });
  }

  [Fact]
  public async Task VerifyPinAsync_WithWrongPin_CountsAttemptAndThrows()
  {
    User user = GivenUser();
    PasswordResetPin pin = GivenActivePin(user);

    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.VerifyPinAsync(new VerifyPinDto { Email = TestData.Email, Pin = "000000" }));

    Assert.Equal(AuthMessages.InvalidOrExpiredPin, ex.Message);
    Assert.Equal(1, pin.FailedAttempts);
    Assert.False(pin.IsUsed);
  }

  [Fact]
  public async Task VerifyPinAsync_OnLastAllowedWrongGuess_CancelsPin()
  {
    User user = GivenUser();
    PasswordResetPin pin = GivenActivePin(user);
    pin.FailedAttempts = _options.MaxResetPinAttempts - 1;

    await Assert.ThrowsAsync<BadRequestException>(
      () => _service.VerifyPinAsync(new VerifyPinDto { Email = TestData.Email, Pin = "000000" }));

    Assert.True(pin.IsUsed);
  }

  [Fact]
  public async Task VerifyPinAsync_WithNoActivePin_Throws()
  {
    GivenUser();
    _resets.Setup(r => r.GetLatestActivePinAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
      .ReturnsAsync((PasswordResetPin?)null);

    await Assert.ThrowsAsync<BadRequestException>(
      () => _service.VerifyPinAsync(new VerifyPinDto { Email = TestData.Email, Pin = Pin }));
  }

  [Fact]
  public async Task VerifyPinAsync_ForUnknownEmail_ThrowsSameMessage()
  {
    _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.VerifyPinAsync(new VerifyPinDto { Email = TestData.Email, Pin = Pin }));

    Assert.Equal(AuthMessages.InvalidOrExpiredPin, ex.Message);
  }

  // ---------------------------------------------------------------- Reset password

  [Fact]
  public async Task ResetPasswordAsync_WithValidRequest_UpdatesPasswordUnlocksAndUsesPin()
  {
    User user = GivenUser();
    user.RequiresPasswordReset = true;
    user.LockoutEndUtc = TestData.Now.UtcDateTime.AddSeconds(-5);
    string oldHash = user.PasswordHash;
    PasswordResetPin pin = GivenActivePin(user);

    await _service.ResetPasswordAsync(Reset(TestData.OtherStrongPassword));

    Assert.NotEqual(oldHash, user.PasswordHash);
    Assert.False(user.RequiresPasswordReset);
    Assert.Null(user.LockoutEndUtc);
    Assert.True(pin.IsUsed);
    _resets.Verify(r => r.AddPasswordHistoryAsync(
      It.Is<PasswordHistory>(h => h.PasswordHash == oldHash && h.UserId == user.Id)), Times.Once);
  }

  [Fact]
  public async Task ResetPasswordAsync_WhenPasswordsDoNotMatch_Throws()
  {
    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(() =>
      _service.ResetPasswordAsync(new ResetPasswordDto
      {
        Email = TestData.Email,
        Pin = Pin,
        NewPassword = TestData.OtherStrongPassword,
        ConfirmPassword = TestData.OtherStrongPassword + "x",
      }));

    Assert.Equal(AuthMessages.PasswordsDoNotMatch, ex.Message);
  }

  [Theory]
  [InlineData("short1!")]
  [InlineData("alllowercase1!")]
  [InlineData("NoDigitsHere!")]
  [InlineData("NoSpecial123")]
  public async Task ResetPasswordAsync_WithWeakPassword_Throws(string weak)
  {
    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.ResetPasswordAsync(Reset(weak)));

    Assert.Equal(AuthMessages.PasswordComplexity, ex.Message);
  }

  [Fact]
  public async Task ResetPasswordAsync_WithCurrentPassword_Throws()
  {
    User user = GivenUser();
    GivenActivePin(user);

    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.ResetPasswordAsync(Reset(TestData.Password)));

    Assert.Equal(AuthMessages.PasswordPreviouslyUsed, ex.Message);
  }

  [Fact]
  public async Task ResetPasswordAsync_WithPasswordFromHistory_Throws()
  {
    User user = GivenUser();
    GivenActivePin(user);
    _resets.Setup(r => r.GetRecentPasswordHashesAsync(user.Id, _options.PasswordHistoryCount))
      .ReturnsAsync([TestData.HashPassword(TestData.OtherStrongPassword)]);

    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => _service.ResetPasswordAsync(Reset(TestData.OtherStrongPassword)));

    Assert.Equal(AuthMessages.PasswordPreviouslyUsed, ex.Message);
  }

  [Fact]
  public async Task ResetPasswordAsync_WithInvalidPin_DoesNotChangePassword()
  {
    User user = GivenUser();
    string oldHash = user.PasswordHash;
    GivenActivePin(user);

    await Assert.ThrowsAsync<BadRequestException>(() =>
      _service.ResetPasswordAsync(new ResetPasswordDto
      {
        Email = TestData.Email,
        Pin = "000000",
        NewPassword = TestData.OtherStrongPassword,
        ConfirmPassword = TestData.OtherStrongPassword,
      }));

    Assert.Equal(oldHash, user.PasswordHash);
  }

  // ---------------------------------------------------------------- Helpers

  private User GivenUser(UserRole role = UserRole.NormalUser)
  {
    User user = TestData.User(role);
    _users.Setup(u => u.GetByEmailAsync(TestData.Email)).ReturnsAsync(user);
    return user;
  }

  private PasswordResetPin GivenActivePin(User user)
  {
    PasswordResetPin pin = new()
    {
      Id = 10,
      UserId = user.Id,
      ExpiresAtUtc = TestData.Now.UtcDateTime.AddMinutes(3),
      CreatedAtUtc = TestData.Now.UtcDateTime,
    };
    pin.PinHash = _pinHasher.HashPassword(pin, Pin);
    _resets.Setup(r => r.GetLatestActivePinAsync(user.Id, It.IsAny<DateTime>())).ReturnsAsync(pin);
    return pin;
  }

  private static LoginRequestDto Login(string email, string password)
  {
    return new LoginRequestDto { Email = email, Password = password };
  }

  private static ResetPasswordDto Reset(string newPassword)
  {
    return new ResetPasswordDto
    {
      Email = TestData.Email,
      Pin = Pin,
      NewPassword = newPassword,
      ConfirmPassword = newPassword,
    };
  }
}

using SingularExpress.Api.DTOs.Auth;

namespace SingularExpress.Api.Interfaces;

/// <summary>
/// Login and forgot-password flow. Methods return on success and throw an
/// <see cref="Exceptions.AppException"/> subclass (with a message from AuthMessages) on failure.
/// </summary>
public interface IAuthService
{
  Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

  /// <summary>Emails a reset PIN if the account exists. Never reveals whether it does.</summary>
  Task ForgotPasswordAsync(ForgotPasswordDto request);

  Task VerifyPinAsync(VerifyPinDto request);

  Task ResetPasswordAsync(ResetPasswordDto request);
}

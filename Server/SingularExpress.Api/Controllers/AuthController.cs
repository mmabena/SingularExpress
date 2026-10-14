using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs;
using SingularExpress.Api.DTOs.Auth;
using SingularExpress.Api.Interfaces;

namespace SingularExpress.Api.Controllers;

/// <summary>
/// Sign in and reset a forgotten password. Same routes as HRConnect (/api/auth/...).
/// <para>
/// Controllers stay thin: validate the DTO (automatic via [ApiController]), call the service,
/// return the result. Errors are thrown by the service and formatted by GlobalExceptionHandler.
/// </para>
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting(Policies.AuthRateLimit)]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;

  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  /// <summary>Signs in with a @singular.co.za email and password and returns a JWT.</summary>
  [HttpPost("login")]
  [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status423Locked)]
  public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
  {
    LoginResponseDto response = await _authService.LoginAsync(request);
    return Ok(response);
  }

  /// <summary>
  /// Emails a 6-digit reset PIN. Always returns the same message, whether or not the
  /// account exists, and never returns the PIN itself.
  /// </summary>
  [HttpPost("forgot-password")]
  [ProducesResponseType<MessageResponseDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto request)
  {
    await _authService.ForgotPasswordAsync(request);
    return Ok(new MessageResponseDto(AuthMessages.ResetPinSentIfAccountExists));
  }

  /// <summary>Checks a reset PIN before the user chooses a new password.</summary>
  [HttpPost("verify-pin")]
  [ProducesResponseType<MessageResponseDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<MessageResponseDto>> VerifyPin([FromBody] VerifyPinDto request)
  {
    await _authService.VerifyPinAsync(request);
    return Ok(new MessageResponseDto(AuthMessages.PinVerified));
  }

  /// <summary>Sets a new password using a valid PIN. Also unlocks a locked account.</summary>
  [HttpPost("reset-password")]
  [ProducesResponseType<MessageResponseDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordDto request)
  {
    await _authService.ResetPasswordAsync(request);
    return Ok(new MessageResponseDto(AuthMessages.PasswordResetSuccessful));
  }
}

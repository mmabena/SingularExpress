using System.ComponentModel.DataAnnotations;
using SingularExpress.Api.Constants.Messages;

namespace SingularExpress.Api.DTOs.Auth;

/// <summary>Body of POST /api/auth/reset-password. The PIN is checked again here on purpose.</summary>
public class ResetPasswordDto
{
  [Required(ErrorMessage = AuthMessages.EmailRequired)]
  [EmailAddress(ErrorMessage = AuthMessages.InvalidEmailFormat)]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = AuthMessages.PinRequired)]
  [RegularExpression(@"^\d{6}$", ErrorMessage = AuthMessages.PinFormat)]
  public string Pin { get; set; } = string.Empty;

  [Required(ErrorMessage = AuthMessages.NewPasswordRequired)]
  public string NewPassword { get; set; } = string.Empty;

  [Required(ErrorMessage = AuthMessages.ConfirmPasswordRequired)]
  public string ConfirmPassword { get; set; } = string.Empty;
}

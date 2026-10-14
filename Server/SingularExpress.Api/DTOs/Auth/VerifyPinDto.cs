using System.ComponentModel.DataAnnotations;
using SingularExpress.Api.Constants.Messages;

namespace SingularExpress.Api.DTOs.Auth;

/// <summary>Body of POST /api/auth/verify-pin.</summary>
public class VerifyPinDto
{
  [Required(ErrorMessage = AuthMessages.EmailRequired)]
  [EmailAddress(ErrorMessage = AuthMessages.InvalidEmailFormat)]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = AuthMessages.PinRequired)]
  [RegularExpression(@"^\d{6}$", ErrorMessage = AuthMessages.PinFormat)]
  public string Pin { get; set; } = string.Empty;
}

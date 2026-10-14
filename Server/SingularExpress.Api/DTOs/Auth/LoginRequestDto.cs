using System.ComponentModel.DataAnnotations;
using SingularExpress.Api.Constants.Messages;

namespace SingularExpress.Api.DTOs.Auth;

/// <summary>Body of POST /api/auth/login.</summary>
public class LoginRequestDto
{
  [Required(ErrorMessage = AuthMessages.EmailRequired)]
  [EmailAddress(ErrorMessage = AuthMessages.InvalidEmailFormat)]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = AuthMessages.PasswordRequired)]
  public string Password { get; set; } = string.Empty;
}

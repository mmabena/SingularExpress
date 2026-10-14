using System.ComponentModel.DataAnnotations;
using SingularExpress.Api.Constants.Messages;

namespace SingularExpress.Api.DTOs.Auth;

/// <summary>Body of POST /api/auth/forgot-password.</summary>
public class ForgotPasswordDto
{
  [Required(ErrorMessage = AuthMessages.EmailRequired)]
  [EmailAddress(ErrorMessage = AuthMessages.InvalidEmailFormat)]
  public string Email { get; set; } = string.Empty;
}

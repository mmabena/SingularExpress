using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Api.Options;

/// <summary>
/// Bound from the "Jwt" section. The secret must come from user-secrets locally and from
/// the platform secret store in CI/production. It is never committed (see SECURITY.md).
/// </summary>
public class JwtOptions
{
  public const string SectionName = "Jwt";

  /// <summary>Base64 or plain text. Must be at least 32 bytes (256 bits) for HS256.</summary>
  [Required]
  public string Secret { get; set; } = string.Empty;

  [Required]
  public string Issuer { get; set; } = string.Empty;

  [Required]
  public string Audience { get; set; } = string.Empty;

  [Range(5, 480)]
  public int ExpiryMinutes { get; set; } = 60;
}

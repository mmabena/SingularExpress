using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Services;

/// <summary>
/// Creates the signed JWT returned at login. Claims: sub (user id), email and role.
/// The same issuer, audience and key are used to validate tokens in AuthenticationExtensions.
/// </summary>
public class TokenService : ITokenService
{
  private readonly JwtOptions _options;
  private readonly TimeProvider _timeProvider;

  public TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
  {
    _options = options.Value;
    _timeProvider = timeProvider;
  }

  public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
  {
    DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
    DateTime expiresAtUtc = nowUtc.AddMinutes(_options.ExpiryMinutes);

    ClaimsIdentity identity = new(
    [
      new Claim(ClaimNames.UserId, user.Id.ToString(CultureInfo.InvariantCulture)),
      new Claim(ClaimNames.Email, user.Email),
      new Claim(ClaimNames.Role, user.Role.ToString()),
    ]);

    SymmetricSecurityKey key = new(JwtKeyHelper.GetKeyBytes(_options.Secret));

    SecurityTokenDescriptor descriptor = new()
    {
      Subject = identity,
      Issuer = _options.Issuer,
      Audience = _options.Audience,
      IssuedAt = nowUtc,
      NotBefore = nowUtc,
      Expires = expiresAtUtc,
      SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
    };

    string token = new JsonWebTokenHandler().CreateToken(descriptor);
    return (token, expiresAtUtc);
  }
}

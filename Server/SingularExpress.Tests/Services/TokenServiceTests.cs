using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Services;
using SingularExpress.Api.Utils;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="TokenService"/>.</summary>
public class TokenServiceTests
{
  // Test-only key. Real keys are never written in code (see SECURITY.md).
  private const string TestSecret = "unit-test-signing-key-not-a-real-secret-0123456789";

  private readonly JwtOptions _options = new()
  {
    Secret = TestSecret,
    Issuer = "SingularExpress",
    Audience = "SingularExpressUsers",
    ExpiryMinutes = 60,
  };

  private readonly TokenService _service;

  public TokenServiceTests()
  {
    _service = new TokenService(TestOptions.Of(_options), new FakeTimeProvider(TestData.Now));
  }

  [Fact]
  public void CreateToken_ContainsUserIdEmailAndRoleClaims()
  {
    User user = TestData.User(UserRole.SuperUser, id: 42);

    (string token, _) = _service.CreateToken(user);
    JsonWebToken jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

    Assert.Equal("42", jwt.Subject);
    Assert.Equal(TestData.Email, jwt.GetClaim("email").Value);
    Assert.Equal("SuperUser", jwt.GetClaim("role").Value);
    Assert.Equal("SingularExpress", jwt.Issuer);
    Assert.Contains("SingularExpressUsers", jwt.Audiences);
  }

  [Fact]
  public void CreateToken_ExpiresAfterConfiguredMinutes()
  {
    (string token, DateTime expiresAtUtc) = _service.CreateToken(TestData.User());
    JsonWebToken jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

    DateTime expected = TestData.Now.UtcDateTime.AddMinutes(60);
    Assert.Equal(expected, expiresAtUtc);
    Assert.Equal(expected, jwt.ValidTo);
  }

  [Fact]
  public async Task CreateToken_IsSignedWithTheConfiguredKey()
  {
    (string token, _) = _service.CreateToken(TestData.User());

    TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(
      token,
      ValidationParameters(TestSecret));

    Assert.True(result.IsValid);
  }

  [Fact]
  public async Task CreateToken_FailsValidationWithADifferentKey()
  {
    (string token, _) = _service.CreateToken(TestData.User());

    TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(
      token,
      ValidationParameters("a-completely-different-test-key-0123456789abcdef"));

    Assert.False(result.IsValid);
  }

  private TokenValidationParameters ValidationParameters(string secret)
  {
    return new TokenValidationParameters
    {
      ValidIssuer = _options.Issuer,
      ValidAudience = _options.Audience,
      IssuerSigningKey = new SymmetricSecurityKey(JwtKeyHelper.GetKeyBytes(secret)),
      // The fake clock is in 2026; only the signature is under test here.
      ValidateLifetime = false,
    };
  }
}

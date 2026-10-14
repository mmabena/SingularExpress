using SingularExpress.Api.Models;

namespace SingularExpress.Api.Interfaces;

/// <summary>Creates signed JWT access tokens.</summary>
public interface ITokenService
{
  (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}

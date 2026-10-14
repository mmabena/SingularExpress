using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Extensions;

/// <summary>
/// JWT validation and the three role policies from HRConnect
/// (SuperUserOnly, NormalUserOnly, SuperOrNormalUser).
/// </summary>
public static class AuthenticationExtensions
{
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
  {
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

    // Configured through options so the validated JwtOptions (from user-secrets) are used.
    services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
      {
        JwtOptions jwt = jwtOptions.Value;

        // Keep claim names exactly as TokenService wrote them ("sub", "email", "role").
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt.Issuer,
          ValidAudience = jwt.Audience,
          IssuerSigningKey = new SymmetricSecurityKey(JwtKeyHelper.GetKeyBytes(jwt.Secret)),
          RoleClaimType = ClaimNames.Role,
          NameClaimType = ClaimNames.Email,
          ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Same { errors: [...] } body as every other error.
        bearer.Events = new JwtBearerEvents
        {
          OnChallenge = async context =>
          {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(
              new ErrorResponseDto { Errors = [GeneralMessages.Unauthenticated] });
          },
          OnForbidden = async context =>
          {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
              new ErrorResponseDto { Errors = [GeneralMessages.Forbidden] });
          },
        };
      });

    services.AddAuthorizationBuilder()
      .AddPolicy(Policies.SuperUserOnly, p => p.RequireRole(Roles.SuperUser))
      .AddPolicy(Policies.NormalUserOnly, p => p.RequireRole(Roles.NormalUser))
      .AddPolicy(Policies.SuperOrNormalUser, p => p.RequireRole(Roles.SuperUser, Roles.NormalUser));

    return services;
  }
}

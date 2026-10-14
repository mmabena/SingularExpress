using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SingularExpress.Api.Extensions;

/// <summary>Swagger UI with a JWT "Authorize" button (Development only, see Program.cs).</summary>
public static class SwaggerExtensions
{
  public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
  {
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new OpenApiInfo { Title = "SingularExpress.Api", Version = "v1" });

      string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
      if (File.Exists(xmlPath))
      {
        c.IncludeXmlComments(xmlPath);
      }

      OpenApiSecurityScheme scheme = new()
      {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token from POST /api/auth/login (without the word Bearer).",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
      };

      c.AddSecurityDefinition("Bearer", scheme);
      c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
    });

    return services;
  }
}

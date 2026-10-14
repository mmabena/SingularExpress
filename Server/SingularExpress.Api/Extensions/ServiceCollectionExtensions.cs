using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SingularExpress.Api.Data;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Repository;
using SingularExpress.Api.Services;

namespace SingularExpress.Api.Extensions;

/// <summary>
/// Dependency-injection registrations. When you add a new service or repository,
/// register it here (one line each), grouped by feature, so Program.cs stays short.
/// </summary>
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Shared
    services.AddSingleton(TimeProvider.System);
    services.AddScoped<DbSeeder>();

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
    services.AddScoped<IFinancialPeriodRepository, FinancialPeriodRepository>();

    // Auth
    services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    services.AddScoped<IPasswordHasher<PasswordResetPin>, PasswordHasher<PasswordResetPin>>();
    services.AddSingleton<IPinGenerator, PinGenerator>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IAuthService, AuthService>();

    // Financial periods
    services.AddScoped<IFinancialPeriodService, FinancialPeriodService>();

    // Email: provider chosen by Email:Provider
    string provider = configuration[$"{EmailOptions.SectionName}:Provider"] ?? EmailOptions.FileProvider;
    if (provider == EmailOptions.SendGridProvider)
    {
      services.AddSingleton<ISendGridClient>(sp =>
        new SendGridClient(sp.GetRequiredService<IOptions<EmailOptions>>().Value.SendGridApiKey));
      services.AddScoped<IEmailService, SendGridEmailService>();
    }
    else
    {
      services.AddScoped<IEmailService, FileEmailService>();
    }

    return services;
  }
}

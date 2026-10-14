using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Extensions;

/// <summary>
/// Binds each appsettings section to its Options class and checks it at start-up.
/// A missing or weak setting stops the API immediately with a clear message,
/// instead of failing later on the first login.
/// </summary>
public static class OptionsExtensions
{
  public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<JwtOptions>()
      .Bind(configuration.GetSection(JwtOptions.SectionName))
      .ValidateDataAnnotations()
      .Validate(
        o => JwtKeyHelper.IsStrongEnough(o.Secret),
        "Jwt:Secret is missing or shorter than 32 bytes. Set it with dotnet user-secrets (see SECURITY.md).")
      .ValidateOnStart();

    services.AddOptions<AuthSecurityOptions>()
      .Bind(configuration.GetSection(AuthSecurityOptions.SectionName))
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddOptions<FinancialPeriodOptions>()
      .Bind(configuration.GetSection(FinancialPeriodOptions.SectionName))
      .ValidateDataAnnotations()
      .Validate(
        o => TimeZoneInfo.TryFindSystemTimeZoneById(o.TimeZoneId, out _),
        "FinancialPeriod:TimeZoneId is not a known time zone.")
      .ValidateOnStart();

    services.AddOptions<EmailOptions>()
      .Bind(configuration.GetSection(EmailOptions.SectionName))
      .ValidateDataAnnotations()
      .Validate(
        o => o.Provider is EmailOptions.FileProvider or EmailOptions.SendGridProvider,
        "Email:Provider must be \"File\" or \"SendGrid\".")
      .Validate(
        o => o.Provider != EmailOptions.SendGridProvider || !string.IsNullOrWhiteSpace(o.SendGridApiKey),
        "Email:SendGridApiKey is required when Email:Provider is SendGrid.")
      .ValidateOnStart();

    services.AddOptions<SeedOptions>()
      .Bind(configuration.GetSection(SeedOptions.SectionName));

    return services;
  }
}

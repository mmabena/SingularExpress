using System.ComponentModel.DataAnnotations;

namespace SingularExpress.Api.Options;

/// <summary>Email settings, bound from "Email".</summary>
public class EmailOptions
{
  public const string SectionName = "Email";
  public const string FileProvider = "File";
  public const string SendGridProvider = "SendGrid";

  /// <summary>
  /// "File" (development): emails are written to <see cref="FileOutputFolder"/> instead of sent.
  /// "SendGrid" (test/production): emails are sent with SendGrid.
  /// </summary>
  [Required]
  public string Provider { get; set; } = FileProvider;

  public string FileOutputFolder { get; set; } = "App_Data/dev-emails";

  [Required]
  [EmailAddress]
  public string FromEmail { get; set; } = "noreply@singular.co.za";

  [Required]
  public string FromName { get; set; } = "SingularExpress";

  /// <summary>Only needed when Provider is "SendGrid". Set it with user-secrets, never in a file.</summary>
  public string SendGridApiKey { get; set; } = string.Empty;
}

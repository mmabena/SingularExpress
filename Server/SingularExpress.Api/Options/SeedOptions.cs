namespace SingularExpress.Api.Options;

/// <summary>
/// Optional first SuperUser for a local database, bound from "Seed".
/// Used in Development only, and only when both values are set with user-secrets.
/// Use a test account, never a real person's details.
/// </summary>
public class SeedOptions
{
  public const string SectionName = "Seed";

  public string SuperUserEmail { get; set; } = string.Empty;

  public string SuperUserPassword { get; set; } = string.Empty;
}

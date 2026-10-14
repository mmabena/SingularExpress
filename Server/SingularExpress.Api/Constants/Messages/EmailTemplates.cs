namespace SingularExpress.Api.Constants.Messages;

/// <summary>
/// Subjects and bodies for system emails. Kept with the other messages so wording
/// changes go through one file.
/// </summary>
public static class EmailTemplates
{
  public const string PasswordResetSubject = "SingularExpress password reset PIN";

  /// <summary>{0} = PIN, {1} = minutes until it expires.</summary>
  public const string PasswordResetBody =
    "Your SingularExpress password reset PIN is: {0}\n\n" +
    "This PIN is valid for {1} minutes and can only be used once.\n" +
    "If you did not ask to reset your password, you can ignore this email.";
}

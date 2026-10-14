namespace SingularExpress.Api.Constants.Messages;

/// <summary>Messages shared by the whole API (errors, auth failures, validation).</summary>
public static class GeneralMessages
{
  public const string UnexpectedError =
    "Something went wrong on our side. Quote the trace id when reporting it.";
  public const string Unauthenticated = "Sign in to continue.";
  public const string Forbidden = "You do not have permission to do this.";
  public const string ValidationFailed = "One or more fields are invalid.";
  public const string TooManyRequests = "Too many requests. Wait a minute and try again.";
  public const string UserNotFound = "User not found.";
}

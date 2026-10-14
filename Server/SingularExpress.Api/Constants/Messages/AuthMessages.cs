namespace SingularExpress.Api.Constants.Messages;

/// <summary>
/// Every message the login and password-reset flow sends back to the Client.
/// <para>
/// Rule for the team: never type a user-facing string inside a service or controller.
/// Add it here and reference the constant, so wording is reviewed in one place and
/// unit tests can assert on the constant instead of a copy of the text.
/// </para>
/// </summary>
public static class AuthMessages
{
  // Login
  public const string EmailRequired = "Email is required.";
  public const string PasswordRequired = "Password is required.";
  public const string InvalidEmailFormat = "Enter a valid email address.";
  public const string InvalidEmailDomain = "Email must be a @singular.co.za address.";

  /// <summary>
  /// Deliberately the same for "no such user" and "wrong password" so nobody can use the
  /// login form to find out which email addresses have accounts.
  /// </summary>
  public const string InvalidCredentials = "Invalid email or password.";

  /// <summary>{0} = lockout length in seconds (AuthSecurity:LockoutSeconds).</summary>
  public const string AccountLockedTooManyAttempts =
    "Account locked for {0} seconds due to multiple failed login attempts.";
  public const string AccountTemporarilyLocked =
    "Account is temporarily locked. Try again in a minute.";
  public const string PasswordResetRequired =
    "Account locked. Please reset your password.";

  // Forgot password / PIN
  /// <summary>Same response whether or not the email has an account (no account enumeration).</summary>
  public const string ResetPinSentIfAccountExists =
    "If an account exists for this email, a reset PIN has been sent to it.";
  public const string PinRequired = "PIN is required.";
  public const string PinFormat = "PIN must be 6 digits.";
  public const string InvalidOrExpiredPin = "Invalid or expired PIN.";
  public const string PinVerified = "PIN verified. You can now reset your password.";

  // Reset password
  public const string NewPasswordRequired = "New password is required.";
  public const string ConfirmPasswordRequired = "Confirm your new password.";
  public const string PasswordsDoNotMatch = "Passwords do not match.";
  public const string PasswordComplexity =
    "Password does not meet complexity requirements. Minimum 8 chars, include uppercase, " +
    "lowercase, digit and special character.";
  public const string PasswordPreviouslyUsed = "You cannot reuse a previously used password.";
  public const string PasswordResetSuccessful = "Password reset successfully.";
}

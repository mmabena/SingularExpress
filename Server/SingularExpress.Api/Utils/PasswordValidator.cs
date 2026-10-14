using System.Text.RegularExpressions;

namespace SingularExpress.Api.Utils;

/// <summary>
/// Password complexity rule copied from HRConnect: at least 8 characters with an uppercase
/// letter, a lowercase letter, a digit and a special character.
/// The Client checks the same rule in src/utils/validators.js; the server check is the one
/// that counts.
/// </summary>
public static partial class PasswordValidator
{
  [GeneratedRegex(
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$",
    RegexOptions.None,
    matchTimeoutMilliseconds: 100)]
  private static partial Regex PasswordRegex();

  public static bool IsValidPassword(string? password)
  {
    if (string.IsNullOrWhiteSpace(password))
    {
      return false;
    }

    return PasswordRegex().IsMatch(password);
  }
}

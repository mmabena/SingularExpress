namespace SingularExpress.Api.Utils;

/// <summary>Small, pure helpers for email addresses.</summary>
public static class EmailHelper
{
  /// <summary>Trims and lower-cases so "John@Singular.co.za " and "john@singular.co.za" match.</summary>
  public static string Normalise(string? email)
  {
    return (email ?? string.Empty).Trim().ToLowerInvariant();
  }

  public static bool HasAllowedDomain(string? email, string allowedDomain)
  {
    string normalised = Normalise(email);
    return normalised.Length > allowedDomain.Length
      && normalised.EndsWith(allowedDomain, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  /// "john.smith@singular.co.za" → "j***@singular.co.za".
  /// Use this whenever an email address has to appear in a log (POPIA: keep PI out of logs).
  /// </summary>
  public static string Mask(string? email)
  {
    string normalised = Normalise(email);
    int at = normalised.IndexOf('@', StringComparison.Ordinal);
    if (at <= 0)
    {
      return "***";
    }

    return string.Concat(normalised.AsSpan(0, 1), "***", normalised.AsSpan(at));
  }
}

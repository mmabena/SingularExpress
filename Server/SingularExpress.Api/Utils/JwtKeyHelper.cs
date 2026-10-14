using System.Text;

namespace SingularExpress.Api.Utils;

/// <summary>
/// Turns the configured JWT secret into key bytes. Accepts base64 (preferred) or plain text,
/// like HRConnect. Used both when creating tokens and when validating them.
/// </summary>
public static class JwtKeyHelper
{
  /// <summary>HS256 needs a key of at least 256 bits.</summary>
  public const int MinimumKeyBytes = 32;

  public static byte[] GetKeyBytes(string secret)
  {
    Span<byte> buffer = new byte[secret.Length];
    if (Convert.TryFromBase64String(secret, buffer, out int written) && written > 0)
    {
      return buffer[..written].ToArray();
    }

    return Encoding.UTF8.GetBytes(secret);
  }

  public static bool IsStrongEnough(string? secret)
  {
    return !string.IsNullOrWhiteSpace(secret) && GetKeyBytes(secret).Length >= MinimumKeyBytes;
  }
}

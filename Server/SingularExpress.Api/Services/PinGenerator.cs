using System.Security.Cryptography;
using SingularExpress.Api.Interfaces;

namespace SingularExpress.Api.Services;

/// <summary>
/// Creates reset PINs with a cryptographically secure random number generator.
/// (HRConnect used <c>new Random()</c>, which is predictable and must not be used for secrets.)
/// </summary>
public class PinGenerator : IPinGenerator
{
  public string Generate(int length)
  {
    if (length is < 4 or > 12)
    {
      throw new ArgumentOutOfRangeException(nameof(length), "PIN length must be 4-12 digits.");
    }

    char[] digits = new char[length];
    for (int i = 0; i < length; i++)
    {
      digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
    }

    return new string(digits);
  }
}

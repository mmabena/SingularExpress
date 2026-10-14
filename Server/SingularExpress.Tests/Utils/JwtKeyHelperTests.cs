using System.Text;
using SingularExpress.Api.Utils;

namespace SingularExpress.Tests.Utils;

/// <summary>Unit tests for <see cref="JwtKeyHelper"/>.</summary>
public class JwtKeyHelperTests
{
  [Fact]
  public void GetKeyBytes_DecodesBase64Secrets()
  {
    byte[] raw = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();

    Assert.Equal(raw, JwtKeyHelper.GetKeyBytes(Convert.ToBase64String(raw)));
  }

  [Fact]
  public void GetKeyBytes_FallsBackToUtf8ForPlainText()
  {
    const string secret = "plain text secret!";

    Assert.Equal(Encoding.UTF8.GetBytes(secret), JwtKeyHelper.GetKeyBytes(secret));
  }

  [Theory]
  [InlineData(null, false)]
  [InlineData("", false)]
  [InlineData("too-short", false)]
  [InlineData("unit-test-signing-key-not-a-real-secret-0123456789", true)]
  public void IsStrongEnough_RequiresAtLeast32Bytes(string? secret, bool expected)
  {
    Assert.Equal(expected, JwtKeyHelper.IsStrongEnough(secret));
  }
}

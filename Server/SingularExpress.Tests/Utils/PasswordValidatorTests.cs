using SingularExpress.Api.Utils;

namespace SingularExpress.Tests.Utils;

/// <summary>Unit tests for <see cref="PasswordValidator"/> (rule copied from HRConnect).</summary>
public class PasswordValidatorTests
{
  [Theory]
  [InlineData("Correct-Horse-1!")]
  [InlineData("Aa1!aaaa")]
  public void IsValidPassword_AcceptsComplexPasswords(string password)
  {
    Assert.True(PasswordValidator.IsValidPassword(password));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("Aa1!aaa")]
  [InlineData("aa1!aaaa")]
  [InlineData("AA1!AAAA")]
  [InlineData("Aa!aaaaa")]
  [InlineData("Aa1aaaaa")]
  public void IsValidPassword_RejectsWeakPasswords(string? password)
  {
    Assert.False(PasswordValidator.IsValidPassword(password));
  }
}

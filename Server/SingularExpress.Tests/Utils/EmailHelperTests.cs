using SingularExpress.Api.Utils;

namespace SingularExpress.Tests.Utils;

/// <summary>Unit tests for <see cref="EmailHelper"/>.</summary>
public class EmailHelperTests
{
  [Fact]
  public void Normalise_TrimsAndLowerCases()
  {
    Assert.Equal("test.user@singular.co.za", EmailHelper.Normalise("  Test.User@SINGULAR.co.za "));
  }

  [Theory]
  [InlineData("test.user@singular.co.za", true)]
  [InlineData("TEST@SINGULAR.CO.ZA", true)]
  [InlineData("test@gmail.com", false)]
  [InlineData("@singular.co.za", false)]
  [InlineData("test@singular.co.za.example.com", false)]
  [InlineData(null, false)]
  public void HasAllowedDomain_OnlyAcceptsSingularAddresses(string? email, bool expected)
  {
    Assert.Equal(expected, EmailHelper.HasAllowedDomain(email, "@singular.co.za"));
  }

  [Theory]
  [InlineData("test.user@singular.co.za", "t***@singular.co.za")]
  [InlineData("not-an-email", "***")]
  [InlineData(null, "***")]
  public void Mask_HidesTheNamePart(string? email, string expected)
  {
    Assert.Equal(expected, EmailHelper.Mask(email));
  }
}

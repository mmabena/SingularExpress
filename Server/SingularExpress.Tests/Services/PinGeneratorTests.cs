using SingularExpress.Api.Services;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="PinGenerator"/>.</summary>
public class PinGeneratorTests
{
  private readonly PinGenerator _generator = new();

  [Theory]
  [InlineData(4)]
  [InlineData(6)]
  [InlineData(12)]
  public void Generate_ReturnsOnlyDigitsOfRequestedLength(int length)
  {
    string pin = _generator.Generate(length);

    Assert.Equal(length, pin.Length);
    Assert.All(pin, c => Assert.True(char.IsAsciiDigit(c)));
  }

  [Theory]
  [InlineData(3)]
  [InlineData(13)]
  public void Generate_WithLengthOutOfRange_Throws(int length)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(length));
  }

  [Fact]
  public void Generate_ProducesDifferentPins()
  {
    HashSet<string> pins = [];
    for (int i = 0; i < 50; i++)
    {
      pins.Add(_generator.Generate(6));
    }

    // 50 random 6-digit PINs being identical is practically impossible.
    Assert.True(pins.Count > 40);
  }
}

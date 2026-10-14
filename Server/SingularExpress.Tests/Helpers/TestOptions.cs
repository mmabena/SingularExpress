using Microsoft.Extensions.Options;

namespace SingularExpress.Tests.Helpers;

/// <summary>Wraps a settings object as IOptions&lt;T&gt; for constructors.</summary>
public static class TestOptions
{
  public static IOptions<T> Of<T>(T value)
    where T : class
  {
    return Microsoft.Extensions.Options.Options.Create(value);
  }
}

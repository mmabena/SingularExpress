using Microsoft.Extensions.Logging;

namespace SingularExpress.Tests.Helpers;

/// <summary>
/// Logger that keeps messages in a list so tests can check what was (and was not) logged,
/// for example that a PIN or a full email address never reaches the log.
/// </summary>
public sealed class ListLogger<T> : ILogger<T>
{
  public List<string> Messages { get; } = [];

  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull
  {
    return null;
  }

  public bool IsEnabled(LogLevel logLevel)
  {
    return true;
  }

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    Messages.Add(formatter(state, exception));
  }
}

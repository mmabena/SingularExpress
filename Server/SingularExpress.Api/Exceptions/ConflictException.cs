namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 409. The request clashes with the current state (e.g. the period already rolled over).</summary>
public class ConflictException : AppException
{
  public ConflictException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 409;
}

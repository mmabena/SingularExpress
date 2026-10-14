namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 401. The caller could not be authenticated (e.g. wrong email or password).</summary>
public class UnauthorizedException : AppException
{
  public UnauthorizedException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 401;
}

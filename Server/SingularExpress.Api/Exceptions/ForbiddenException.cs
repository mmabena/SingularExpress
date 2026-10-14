namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 403. The caller is signed in but not allowed to do this.</summary>
public class ForbiddenException : AppException
{
  public ForbiddenException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 403;
}

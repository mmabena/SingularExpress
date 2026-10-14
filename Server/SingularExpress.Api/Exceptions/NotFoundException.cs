namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 404. The requested record does not exist.</summary>
public class NotFoundException : AppException
{
  public NotFoundException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 404;
}

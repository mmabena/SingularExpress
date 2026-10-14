namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 400. The request is invalid (bad input or a business rule failed).</summary>
public class BadRequestException : AppException
{
  public BadRequestException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 400;
}

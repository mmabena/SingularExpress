namespace SingularExpress.Api.Exceptions;

/// <summary>
/// Base class for expected errors. Services throw a subclass with a message from
/// Constants/Messages, and <see cref="Middleware.GlobalExceptionHandler"/> turns it into
/// <c>{ "errors": ["message"] }</c> with the right HTTP status code.
/// Controllers therefore never need try/catch.
/// </summary>
public abstract class AppException : Exception
{
  protected AppException(string message)
    : base(message)
  {
  }

  public abstract int StatusCode { get; }
}

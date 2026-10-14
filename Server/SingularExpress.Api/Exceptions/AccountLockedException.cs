namespace SingularExpress.Api.Exceptions;

/// <summary>HTTP 423. The account is locked (too many failed logins or a reset is required).</summary>
public class AccountLockedException : AppException
{
  public AccountLockedException(string message)
    : base(message)
  {
  }

  public override int StatusCode => 423;
}

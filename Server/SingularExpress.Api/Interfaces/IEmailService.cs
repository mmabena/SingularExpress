namespace SingularExpress.Api.Interfaces;

/// <summary>
/// Sends an email. Two implementations: FileEmailService (development) and
/// SendGridEmailService (test/production), chosen by Email:Provider.
/// </summary>
public interface IEmailService
{
  Task SendAsync(string toEmail, string subject, string body);
}

using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Services;

/// <summary>
/// Sends email through SendGrid (same provider as HRConnect). Used when Email:Provider is
/// "SendGrid". The API key comes from user-secrets or the environment variable
/// Email__SendGridApiKey, never from a committed file.
/// </summary>
public class SendGridEmailService : IEmailService
{
  private readonly ISendGridClient _client;
  private readonly EmailOptions _options;
  private readonly ILogger<SendGridEmailService> _logger;

  public SendGridEmailService(
    ISendGridClient client,
    IOptions<EmailOptions> options,
    ILogger<SendGridEmailService> logger)
  {
    _client = client;
    _options = options.Value;
    _logger = logger;
  }

  public async Task SendAsync(string toEmail, string subject, string body)
  {
    SendGridMessage message = MailHelper.CreateSingleEmail(
      new EmailAddress(_options.FromEmail, _options.FromName),
      new EmailAddress(toEmail),
      subject,
      body,
      htmlContent: null);

    Response response = await _client.SendEmailAsync(message);

    if (!response.IsSuccessStatusCode)
    {
      // Log status only: never the body (it can contain a PIN) or the full address.
      _logger.LogError(
        "SendGrid returned {StatusCode} sending to {Recipient}",
        (int)response.StatusCode,
        EmailHelper.Mask(toEmail));
      throw new InvalidOperationException("Email could not be sent.");
    }
  }
}

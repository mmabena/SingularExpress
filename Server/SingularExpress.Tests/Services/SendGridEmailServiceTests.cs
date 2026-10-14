using System.Net;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using SingularExpress.Api.Options;
using SingularExpress.Api.Services;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="SendGridEmailService"/>. SendGrid itself is mocked.</summary>
public class SendGridEmailServiceTests
{
  private readonly Mock<ISendGridClient> _client = new();
  private readonly ListLogger<SendGridEmailService> _logger = new();
  private readonly SendGridEmailService _service;

  public SendGridEmailServiceTests()
  {
    _service = new SendGridEmailService(
      _client.Object,
      TestOptions.Of(new EmailOptions { Provider = EmailOptions.SendGridProvider, SendGridApiKey = "test" }),
      _logger);
  }

  [Fact]
  public async Task SendAsync_SendsOneMessageToTheRecipient()
  {
    SendGridMessage? sent = null;
    _client.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
      .Callback<SendGridMessage, CancellationToken>((m, _) => sent = m)
      .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

    await _service.SendAsync(TestData.Email, "Subject", "Body");

    Assert.NotNull(sent);
    Assert.Equal(TestData.Email, sent.Personalizations[0].Tos[0].Email);
    Assert.Equal("Subject", sent.Subject);
  }

  [Fact]
  public async Task SendAsync_WhenSendGridFails_ThrowsAndLogsMaskedAddress()
  {
    _client.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Response(HttpStatusCode.BadRequest, null, null));

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => _service.SendAsync(TestData.Email, "Subject", "Body"));

    string log = string.Join('\n', _logger.Messages);
    Assert.Contains("t***@singular.co.za", log, StringComparison.Ordinal);
    Assert.DoesNotContain(TestData.Email, log, StringComparison.Ordinal);
  }
}

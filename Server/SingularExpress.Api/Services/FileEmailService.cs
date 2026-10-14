using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Services;

/// <summary>
/// DEVELOPMENT ONLY. Writes each email to a text file in Email:FileOutputFolder
/// (App_Data/dev-emails by default, which is git-ignored) instead of sending it.
/// Open the newest file to read your reset PIN. The PIN is never written to the log.
/// </summary>
public class FileEmailService : IEmailService
{
  private readonly EmailOptions _options;
  private readonly IHostEnvironment _environment;
  private readonly TimeProvider _timeProvider;
  private readonly ILogger<FileEmailService> _logger;

  public FileEmailService(
    IOptions<EmailOptions> options,
    IHostEnvironment environment,
    TimeProvider timeProvider,
    ILogger<FileEmailService> logger)
  {
    _options = options.Value;
    _environment = environment;
    _timeProvider = timeProvider;
    _logger = logger;
  }

  public async Task SendAsync(string toEmail, string subject, string body)
  {
    string folder = Path.Combine(_environment.ContentRootPath, _options.FileOutputFolder);
    Directory.CreateDirectory(folder);

    string timestamp = _timeProvider.GetUtcNow().ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture);
    string fileName = $"{timestamp}-{Guid.NewGuid():N}.txt";
    string filePath = Path.Combine(folder, fileName);

    StringBuilder content = new();
    content.AppendLine(CultureInfo.InvariantCulture, $"From: {_options.FromName} <{_options.FromEmail}>");
    content.AppendLine(CultureInfo.InvariantCulture, $"To: {toEmail}");
    content.AppendLine(CultureInfo.InvariantCulture, $"Subject: {subject}");
    content.AppendLine();
    content.AppendLine(body);

    await File.WriteAllTextAsync(filePath, content.ToString());

    _logger.LogInformation(
      "Development email for {Recipient} written to {FileName}",
      EmailHelper.Mask(toEmail),
      fileName);
  }
}

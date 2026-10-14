using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Moq;
using SingularExpress.Api.Options;
using SingularExpress.Api.Services;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="FileEmailService"/>. Uses a temporary folder.</summary>
public sealed class FileEmailServiceTests : IDisposable
{
  private readonly string _root = Path.Combine(Path.GetTempPath(), "se-email-tests-" + Guid.NewGuid().ToString("N"));
  private readonly ListLogger<FileEmailService> _logger = new();
  private readonly FileEmailService _service;

  public FileEmailServiceTests()
  {
    Mock<IHostEnvironment> environment = new();
    environment.SetupGet(e => e.ContentRootPath).Returns(_root);
    environment.SetupGet(e => e.ContentRootFileProvider).Returns(new NullFileProvider());

    _service = new FileEmailService(
      TestOptions.Of(new EmailOptions { FileOutputFolder = "dev-emails" }),
      environment.Object,
      new FakeTimeProvider(TestData.Now),
      _logger);
  }

  [Fact]
  public async Task SendAsync_WritesEmailToFile()
  {
    await _service.SendAsync(TestData.Email, "Subject line", "Body with PIN 123456");

    string file = Assert.Single(Directory.GetFiles(Path.Combine(_root, "dev-emails")));
    string content = await File.ReadAllTextAsync(file);
    Assert.Contains("Subject: Subject line", content, StringComparison.Ordinal);
    Assert.Contains("Body with PIN 123456", content, StringComparison.Ordinal);
  }

  [Fact]
  public async Task SendAsync_NeverLogsTheBodyOrFullEmailAddress()
  {
    await _service.SendAsync(TestData.Email, "Subject line", "Body with PIN 123456");

    string log = string.Join('\n', _logger.Messages);
    Assert.DoesNotContain("123456", log, StringComparison.Ordinal);
    Assert.DoesNotContain(TestData.Email, log, StringComparison.Ordinal);
  }

  public void Dispose()
  {
    if (Directory.Exists(_root))
    {
      Directory.Delete(_root, recursive: true);
    }
  }
}

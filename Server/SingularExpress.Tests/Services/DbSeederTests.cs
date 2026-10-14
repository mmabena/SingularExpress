using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using SingularExpress.Api.Data;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>Unit tests for <see cref="DbSeeder"/> (start-up data).</summary>
public class DbSeederTests
{
  private readonly Mock<IFinancialPeriodService> _periods = new();
  private readonly Mock<IUserRepository> _users = new();
  private readonly Mock<IHostEnvironment> _environment = new();
  private readonly SeedOptions _seed = new()
  {
    SuperUserEmail = "Seed.Admin@singular.co.za",
    SuperUserPassword = TestData.Password,
  };

  [Fact]
  public async Task SeedAsync_AlwaysEnsuresAnOpenFinancialPeriod()
  {
    GivenEnvironment("Production");

    await CreateSeeder().SeedAsync();

    _periods.Verify(p => p.EnsureInitialPeriodAsync(), Times.Once);
  }

  [Fact]
  public async Task SeedAsync_InDevelopmentWithSettings_CreatesHashedSuperUser()
  {
    GivenEnvironment("Development");
    User? created = null;
    _users.Setup(u => u.AnyWithRoleAsync(UserRole.SuperUser)).ReturnsAsync(false);
    _users.Setup(u => u.AddAsync(It.IsAny<User>())).Callback<User>(u => created = u).Returns(Task.CompletedTask);

    await CreateSeeder().SeedAsync();

    Assert.NotNull(created);
    Assert.Equal("seed.admin@singular.co.za", created.Email);
    Assert.Equal(UserRole.SuperUser, created.Role);
    Assert.NotEqual(TestData.Password, created.PasswordHash);
  }

  [Fact]
  public async Task SeedAsync_InProduction_NeverCreatesUsers()
  {
    GivenEnvironment("Production");

    await CreateSeeder().SeedAsync();

    _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task SeedAsync_WhenSuperUserAlreadyExists_DoesNotCreateAnother()
  {
    GivenEnvironment("Development");
    _users.Setup(u => u.AnyWithRoleAsync(UserRole.SuperUser)).ReturnsAsync(true);

    await CreateSeeder().SeedAsync();

    _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task SeedAsync_WithWeakPassword_SkipsUser()
  {
    GivenEnvironment("Development");
    _seed.SuperUserPassword = "weak";
    _users.Setup(u => u.AnyWithRoleAsync(UserRole.SuperUser)).ReturnsAsync(false);

    await CreateSeeder().SeedAsync();

    _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
  }

  private void GivenEnvironment(string name)
  {
    _environment.SetupGet(e => e.EnvironmentName).Returns(name);
  }

  private DbSeeder CreateSeeder()
  {
    return new DbSeeder(
      _periods.Object,
      _users.Object,
      new PasswordHasher<User>(),
      _environment.Object,
      TestOptions.Of(_seed),
      TestOptions.Of(new AuthSecurityOptions()),
      new FakeTimeProvider(TestData.Now),
      NullLogger<DbSeeder>.Instance);
  }
}

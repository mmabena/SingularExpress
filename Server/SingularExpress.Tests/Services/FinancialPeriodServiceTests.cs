using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs.FinancialPeriod;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Services;
using SingularExpress.Tests.Helpers;

namespace SingularExpress.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FinancialPeriodService"/>: current period, year overview,
/// the SuperUser-only rollover and first-run set-up. Today is 13 October 2026 (period 7).
/// </summary>
public class FinancialPeriodServiceTests
{
  private const int SuperUserId = 1;
  private const int NormalUserId = 2;

  private readonly Mock<IFinancialPeriodRepository> _periods = new();
  private readonly Mock<IUserRepository> _users = new();
  private readonly FakeTimeProvider _time = new(TestData.Now);
  private readonly FinancialPeriodOptions _options = new() { AllowRolloverBeforePeriodEnd = true };

  private FinancialPeriod? _closed;
  private FinancialPeriod? _opened;
  private AuditLog? _audit;

  public FinancialPeriodServiceTests()
  {
    _users.Setup(u => u.GetByIdAsync(SuperUserId)).ReturnsAsync(TestData.User(UserRole.SuperUser, SuperUserId));
    _users.Setup(u => u.GetByIdAsync(NormalUserId)).ReturnsAsync(TestData.User(UserRole.NormalUser, NormalUserId));

    _periods.Setup(p => p.CommitRolloverAsync(
        It.IsAny<FinancialPeriod>(), It.IsAny<FinancialPeriod>(), It.IsAny<AuditLog>()))
      .Callback<FinancialPeriod, FinancialPeriod, AuditLog>((c, o, a) =>
      {
        _closed = c;
        _opened = o;
        _audit = a;
      })
      .Returns(Task.CompletedTask);
  }

  // ---------------------------------------------------------------- Reading

  [Fact]
  public async Task GetCurrentPeriodAsync_ReturnsOctober2026AsPeriod7Of2026To27()
  {
    GivenOpenPeriod(TestData.Period(2026, 10));

    FinancialPeriodDto dto = await CreateService().GetCurrentPeriodAsync();

    Assert.Equal("2026/27", dto.FinancialYear);
    Assert.Equal(7, dto.PeriodNumber);
    Assert.Equal("October", dto.MonthName);
    Assert.Equal("2026/27 P07 (October 2026)", dto.Label);
    Assert.Equal(new DateOnly(2026, 10, 31), dto.EndDate);
  }

  [Fact]
  public async Task GetCurrentPeriodAsync_WhenNoneOpen_ThrowsNotFound()
  {
    _periods.Setup(p => p.GetOpenPeriodAsync()).ReturnsAsync((FinancialPeriod?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetCurrentPeriodAsync());
  }

  [Fact]
  public async Task GetFinancialYearOverviewAsync_ReturnsAprilToMarchWithStatuses()
  {
    _periods.Setup(p => p.GetByFinancialYearAsync(2026)).ReturnsAsync(
    [
      TestData.Period(2026, 9, id: 6, status: PeriodStatus.Closed),
      TestData.Period(2026, 10, id: 7, status: PeriodStatus.Open),
    ]);

    FinancialYearOverviewDto overview = await CreateService().GetFinancialYearOverviewAsync(2026);

    Assert.Equal(12, overview.Periods.Count);
    Assert.Equal("April", overview.Periods[0].MonthName);
    Assert.Equal(2026, overview.Periods[0].CalendarYear);
    Assert.Equal("March", overview.Periods[11].MonthName);
    Assert.Equal(2027, overview.Periods[11].CalendarYear);
    Assert.Equal("Closed", overview.Periods[5].Status);
    Assert.Equal("Open", overview.Periods[6].Status);
    Assert.Equal("Upcoming", overview.Periods[7].Status);
  }

  [Theory]
  [InlineData(1999)]
  [InlineData(2101)]
  public async Task GetFinancialYearOverviewAsync_WithYearOutOfRange_ThrowsBadRequest(int year)
  {
    await Assert.ThrowsAsync<BadRequestException>(() => CreateService().GetFinancialYearOverviewAsync(year));
  }

  // ---------------------------------------------------------------- Rollover: who may do it

  [Fact]
  public async Task RolloverAsync_ByNormalUser_ThrowsForbiddenAndChangesNothing()
  {
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    ForbiddenException ex = await Assert.ThrowsAsync<ForbiddenException>(
      () => CreateService().RolloverAsync(Expect(7), NormalUserId));

    Assert.Equal(FinancialPeriodMessages.OnlySuperUserCanRollover, ex.Message);
    VerifyNothingSaved();
  }

  [Fact]
  public async Task RolloverAsync_ByUnknownUser_ThrowsForbidden()
  {
    _users.Setup(u => u.GetByIdAsync(99)).ReturnsAsync((User?)null);

    await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().RolloverAsync(Expect(7), 99));
  }

  [Fact]
  public async Task RolloverAsync_ByInactiveSuperUser_ThrowsForbidden()
  {
    User inactive = TestData.User(UserRole.SuperUser, id: 5);
    inactive.IsActive = false;
    _users.Setup(u => u.GetByIdAsync(5)).ReturnsAsync(inactive);

    await Assert.ThrowsAsync<ForbiddenException>(() => CreateService().RolloverAsync(Expect(7), 5));
  }

  // ---------------------------------------------------------------- Rollover: state checks

  [Fact]
  public async Task RolloverAsync_WhenNoPeriodIsOpen_ThrowsConflict()
  {
    _periods.Setup(p => p.GetOpenPeriodAsync()).ReturnsAsync((FinancialPeriod?)null);

    await Assert.ThrowsAsync<ConflictException>(() => CreateService().RolloverAsync(Expect(7), SuperUserId));
  }

  [Fact]
  public async Task RolloverAsync_WhenClientExpectedAnOlderPeriod_ThrowsConflict()
  {
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    ConflictException ex = await Assert.ThrowsAsync<ConflictException>(
      () => CreateService().RolloverAsync(Expect(6), SuperUserId));

    Assert.Equal(FinancialPeriodMessages.PeriodChangedSinceLoaded, ex.Message);
    VerifyNothingSaved();
  }

  [Fact]
  public async Task RolloverAsync_WhenNextPeriodAlreadyExists_ThrowsConflict()
  {
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));
    _periods.Setup(p => p.ExistsAsync(2026, 8)).ReturnsAsync(true);

    await Assert.ThrowsAsync<ConflictException>(() => CreateService().RolloverAsync(Expect(7), SuperUserId));
    VerifyNothingSaved();
  }

  [Fact]
  public async Task RolloverAsync_BeforePeriodEnd_WhenEarlyRolloverDisabled_ThrowsBadRequest()
  {
    _options.AllowRolloverBeforePeriodEnd = false;
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(
      () => CreateService().RolloverAsync(Expect(7), SuperUserId));

    Assert.Contains("31 Oct 2026", ex.Message, StringComparison.Ordinal);
    VerifyNothingSaved();
  }

  [Fact]
  public async Task RolloverAsync_JustAfterMidnightInSouthAfrica_WhenEarlyRolloverDisabled_Succeeds()
  {
    // 31 Oct 2026 22:30 UTC is already 1 Nov 2026 00:30 in Johannesburg (UTC+2).
    _time.SetUtcNow(new DateTimeOffset(2026, 10, 31, 22, 30, 0, TimeSpan.Zero));
    _options.AllowRolloverBeforePeriodEnd = false;
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    RolloverResultDto result = await CreateService().RolloverAsync(Expect(7), SuperUserId);

    Assert.Equal(8, result.OpenedPeriod.PeriodNumber);
  }

  // ---------------------------------------------------------------- Rollover: results

  [Fact]
  public async Task RolloverAsync_FromOctober_ClosesP07AndOpensNovemberP08()
  {
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    RolloverResultDto result = await CreateService().RolloverAsync(Expect(7), SuperUserId);

    Assert.NotNull(_closed);
    Assert.Equal(PeriodStatus.Closed, _closed.Status);
    Assert.Equal(SuperUserId, _closed.ClosedByUserId);
    Assert.Equal(TestData.Now.UtcDateTime, _closed.ClosedAtUtc);

    Assert.NotNull(_opened);
    Assert.Equal(PeriodStatus.Open, _opened.Status);
    Assert.Equal(2026, _opened.FinancialYearStart);
    Assert.Equal(8, _opened.PeriodNumber);
    Assert.Equal(11, _opened.CalendarMonth);
    Assert.Equal(new DateOnly(2026, 11, 30), _opened.EndDate);

    Assert.False(result.StartedNewFinancialYear);
    Assert.Equal("Closed 2026/27 P07 (October 2026). 2026/27 P08 (November 2026) is now open.", result.Message);
  }

  [Fact]
  public async Task RolloverAsync_FromDecember_OpensJanuaryOfNextCalendarYearInSameFinancialYear()
  {
    GivenOpenPeriod(TestData.Period(2026, 12, id: 9));

    RolloverResultDto result = await CreateService().RolloverAsync(Expect(9), SuperUserId);

    Assert.Equal(2026, result.OpenedPeriod.FinancialYearStart);
    Assert.Equal(10, result.OpenedPeriod.PeriodNumber);
    Assert.Equal(2027, result.OpenedPeriod.CalendarYear);
    Assert.Equal(1, result.OpenedPeriod.CalendarMonth);
    Assert.False(result.StartedNewFinancialYear);
  }

  [Fact]
  public async Task RolloverAsync_FromMarch_StartsNewFinancialYearInApril()
  {
    GivenOpenPeriod(TestData.Period(2027, 3, id: 12));

    RolloverResultDto result = await CreateService().RolloverAsync(Expect(12), SuperUserId);

    Assert.True(result.StartedNewFinancialYear);
    Assert.Equal("2027/28", result.OpenedPeriod.FinancialYear);
    Assert.Equal(1, result.OpenedPeriod.PeriodNumber);
    Assert.Equal(4, result.OpenedPeriod.CalendarMonth);
    Assert.Equal(2027, result.OpenedPeriod.CalendarYear);
  }

  [Fact]
  public async Task RolloverAsync_WritesAuditEntryWithUserIdAndNoPersonalData()
  {
    GivenOpenPeriod(TestData.Period(2026, 10, id: 7));

    await CreateService().RolloverAsync(Expect(7), SuperUserId);

    Assert.NotNull(_audit);
    Assert.Equal(AuditActions.FinancialPeriodRollover, _audit.Action);
    Assert.Equal(SuperUserId, _audit.PerformedByUserId);
    Assert.Equal("7", _audit.EntityId);
    Assert.DoesNotContain("@", _audit.Details, StringComparison.Ordinal);
  }

  // ---------------------------------------------------------------- First-run set-up

  [Fact]
  public async Task EnsureInitialPeriodAsync_WhenPeriodsExist_DoesNothing()
  {
    _periods.Setup(p => p.AnyAsync()).ReturnsAsync(true);

    await CreateService().EnsureInitialPeriodAsync();

    _periods.Verify(p => p.AddInitialPeriodAsync(It.IsAny<FinancialPeriod>(), It.IsAny<AuditLog>()), Times.Never);
  }

  [Fact]
  public async Task EnsureInitialPeriodAsync_OnEmptyDatabase_OpensOctober2026AsP07()
  {
    FinancialPeriod? created = null;
    _periods.Setup(p => p.AnyAsync()).ReturnsAsync(false);
    _periods.Setup(p => p.AddInitialPeriodAsync(It.IsAny<FinancialPeriod>(), It.IsAny<AuditLog>()))
      .Callback<FinancialPeriod, AuditLog>((p, _) => created = p)
      .Returns(Task.CompletedTask);

    await CreateService().EnsureInitialPeriodAsync();

    Assert.NotNull(created);
    Assert.Equal(2026, created.FinancialYearStart);
    Assert.Equal(7, created.PeriodNumber);
    Assert.Equal(PeriodStatus.Open, created.Status);
  }

  [Fact]
  public async Task EnsureInitialPeriodAsync_WithJanuaryConfigured_UsesPreviousFinancialYear()
  {
    _options.InitialPeriod = "2027-01";
    FinancialPeriod? created = null;
    _periods.Setup(p => p.AnyAsync()).ReturnsAsync(false);
    _periods.Setup(p => p.AddInitialPeriodAsync(It.IsAny<FinancialPeriod>(), It.IsAny<AuditLog>()))
      .Callback<FinancialPeriod, AuditLog>((p, _) => created = p)
      .Returns(Task.CompletedTask);

    await CreateService().EnsureInitialPeriodAsync();

    Assert.NotNull(created);
    Assert.Equal(2026, created.FinancialYearStart);
    Assert.Equal(10, created.PeriodNumber);
  }

  // ---------------------------------------------------------------- Helpers

  private FinancialPeriodService CreateService()
  {
    return new FinancialPeriodService(
      _periods.Object,
      _users.Object,
      _time,
      TestOptions.Of(_options),
      NullLogger<FinancialPeriodService>.Instance);
  }

  private void GivenOpenPeriod(FinancialPeriod period)
  {
    _periods.Setup(p => p.GetOpenPeriodAsync()).ReturnsAsync(period);
  }

  private static RolloverRequestDto Expect(int periodId)
  {
    return new RolloverRequestDto { ExpectedCurrentPeriodId = periodId };
  }

  private void VerifyNothingSaved()
  {
    _periods.Verify(
      p => p.CommitRolloverAsync(It.IsAny<FinancialPeriod>(), It.IsAny<FinancialPeriod>(), It.IsAny<AuditLog>()),
      Times.Never);
  }
}

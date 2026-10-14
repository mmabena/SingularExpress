using System.Globalization;
using Microsoft.Extensions.Options;
using SingularExpress.Api.Constants;
using SingularExpress.Api.Constants.Messages;
using SingularExpress.Api.DTOs.FinancialPeriod;
using SingularExpress.Api.Exceptions;
using SingularExpress.Api.Interfaces;
using SingularExpress.Api.Mappers;
using SingularExpress.Api.Models;
using SingularExpress.Api.Options;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Services;

/// <summary>
/// Financial periods run April (P01) to March (P12). Exactly one period is open.
/// <para>
/// Rollover is MANUAL and SuperUser-only (HRConnect rolled over automatically on the 1st of
/// each month; SingularExpress does not). Rollover rules, in order:
/// </para>
/// <list type="number">
/// <item>The caller must be an active SuperUser (also enforced on the controller).</item>
/// <item>There must be an open period, and it must be the one the Client expected.</item>
/// <item>Unless AllowRolloverBeforePeriodEnd is on, the period's last day must have passed.</item>
/// <item>The open period is closed, the next one opened (P12 → P01 of the next year) and an
/// audit entry written, all in one transaction.</item>
/// </list>
/// See docs/FINANCIAL_PERIODS.md.
/// </summary>
public class FinancialPeriodService : IFinancialPeriodService
{
  private readonly IFinancialPeriodRepository _periodRepository;
  private readonly IUserRepository _userRepository;
  private readonly TimeProvider _timeProvider;
  private readonly FinancialPeriodOptions _options;
  private readonly ILogger<FinancialPeriodService> _logger;

  public FinancialPeriodService(
    IFinancialPeriodRepository periodRepository,
    IUserRepository userRepository,
    TimeProvider timeProvider,
    IOptions<FinancialPeriodOptions> options,
    ILogger<FinancialPeriodService> logger)
  {
    _periodRepository = periodRepository;
    _userRepository = userRepository;
    _timeProvider = timeProvider;
    _options = options.Value;
    _logger = logger;
  }

  public async Task<FinancialPeriodDto> GetCurrentPeriodAsync()
  {
    FinancialPeriod period = await _periodRepository.GetOpenPeriodAsync()
      ?? throw new NotFoundException(FinancialPeriodMessages.NoOpenPeriod);

    return period.ToDto();
  }

  public async Task<FinancialYearOverviewDto> GetFinancialYearOverviewAsync(int financialYearStart)
  {
    if (financialYearStart is < 2000 or > 2100)
    {
      throw new BadRequestException(FinancialPeriodMessages.InvalidFinancialYear);
    }

    IReadOnlyList<FinancialPeriod> stored =
      await _periodRepository.GetByFinancialYearAsync(financialYearStart);

    List<PeriodSlotDto> slots = [];
    for (int periodNumber = 1; periodNumber <= FinancialYearHelper.PeriodsPerYear; periodNumber++)
    {
      FinancialPeriod? match = stored.FirstOrDefault(p => p.PeriodNumber == periodNumber);
      int month = FinancialYearHelper.GetCalendarMonth(periodNumber);

      slots.Add(new PeriodSlotDto
      {
        PeriodNumber = periodNumber,
        CalendarYear = FinancialYearHelper.GetCalendarYear(financialYearStart, periodNumber),
        CalendarMonth = month,
        MonthName = FinancialYearHelper.GetMonthName(month),
        Status = match?.Status.ToString() ?? "Upcoming",
      });
    }

    return new FinancialYearOverviewDto
    {
      FinancialYear = FinancialYearHelper.FormatFinancialYear(financialYearStart),
      FinancialYearStart = financialYearStart,
      Periods = slots,
    };
  }

  public async Task<RolloverResultDto> RolloverAsync(RolloverRequestDto request, int performedByUserId)
  {
    await EnsureCallerIsSuperUserAsync(performedByUserId);

    FinancialPeriod current = await _periodRepository.GetOpenPeriodAsync()
      ?? throw new ConflictException(FinancialPeriodMessages.NoOpenPeriod);

    if (current.Id != request.ExpectedCurrentPeriodId)
    {
      throw new ConflictException(FinancialPeriodMessages.PeriodChangedSinceLoaded);
    }

    if (!_options.AllowRolloverBeforePeriodEnd && GetLocalToday() <= current.EndDate)
    {
      throw new BadRequestException(string.Format(
        CultureInfo.InvariantCulture,
        FinancialPeriodMessages.PeriodNotEnded,
        current.EndDate.ToString("d MMM yyyy", CultureInfo.InvariantCulture)));
    }

    (int nextYear, int nextPeriod) =
      FinancialYearHelper.GetNextPeriod(current.FinancialYearStart, current.PeriodNumber);

    if (await _periodRepository.ExistsAsync(nextYear, nextPeriod))
    {
      throw new ConflictException(FinancialPeriodMessages.NextPeriodAlreadyExists);
    }

    DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

    current.Status = PeriodStatus.Closed;
    current.ClosedAtUtc = nowUtc;
    current.ClosedByUserId = performedByUserId;

    FinancialPeriod opened = CreatePeriod(nextYear, nextPeriod, nowUtc);

    string closedLabel = FinancialYearHelper.FormatPeriodLabel(current.FinancialYearStart, current.PeriodNumber);
    string openedLabel = FinancialYearHelper.FormatPeriodLabel(nextYear, nextPeriod);

    AuditLog audit = new()
    {
      Action = AuditActions.FinancialPeriodRollover,
      EntityName = nameof(FinancialPeriod),
      EntityId = current.Id.ToString(CultureInfo.InvariantCulture),
      PerformedByUserId = performedByUserId,
      Details = $"Closed {closedLabel}; opened {openedLabel}.",
      CreatedAtUtc = nowUtc,
    };

    await _periodRepository.CommitRolloverAsync(current, opened, audit);

    _logger.LogInformation(
      "User {UserId} rolled over {ClosedPeriod} to {OpenedPeriod}",
      performedByUserId,
      closedLabel,
      openedLabel);

    return new RolloverResultDto
    {
      ClosedPeriod = current.ToDto(),
      OpenedPeriod = opened.ToDto(),
      StartedNewFinancialYear = nextYear != current.FinancialYearStart,
      Message = string.Format(
        CultureInfo.InvariantCulture,
        FinancialPeriodMessages.RolloverSuccessful,
        closedLabel,
        openedLabel),
    };
  }

  public async Task EnsureInitialPeriodAsync()
  {
    if (await _periodRepository.AnyAsync())
    {
      return;
    }

    (int calendarYear, int calendarMonth) = ParseInitialPeriod(_options.InitialPeriod);
    int financialYearStart = FinancialYearHelper.GetFinancialYearStart(calendarYear, calendarMonth);
    int periodNumber = FinancialYearHelper.GetPeriodNumber(calendarMonth);
    DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

    FinancialPeriod period = CreatePeriod(financialYearStart, periodNumber, nowUtc);
    string label = FinancialYearHelper.FormatPeriodLabel(financialYearStart, periodNumber);

    AuditLog audit = new()
    {
      Action = AuditActions.FinancialPeriodInitialised,
      EntityName = nameof(FinancialPeriod),
      EntityId = label,
      PerformedByUserId = null,
      Details = $"First period opened: {label}.",
      CreatedAtUtc = nowUtc,
    };

    await _periodRepository.AddInitialPeriodAsync(period, audit);
    _logger.LogInformation("Initial financial period opened: {Period}", label);
  }

  /// <summary>Second line of defence: the controller policy already requires SuperUser.</summary>
  private async Task EnsureCallerIsSuperUserAsync(int userId)
  {
    User? user = await _userRepository.GetByIdAsync(userId);
    if (user is null || !user.IsActive || user.Role != UserRole.SuperUser)
    {
      throw new ForbiddenException(FinancialPeriodMessages.OnlySuperUserCanRollover);
    }
  }

  private static FinancialPeriod CreatePeriod(int financialYearStart, int periodNumber, DateTime nowUtc)
  {
    return new FinancialPeriod
    {
      FinancialYearStart = financialYearStart,
      PeriodNumber = periodNumber,
      CalendarYear = FinancialYearHelper.GetCalendarYear(financialYearStart, periodNumber),
      CalendarMonth = FinancialYearHelper.GetCalendarMonth(periodNumber),
      StartDate = FinancialYearHelper.GetStartDate(financialYearStart, periodNumber),
      EndDate = FinancialYearHelper.GetEndDate(financialYearStart, periodNumber),
      Status = PeriodStatus.Open,
      OpenedAtUtc = nowUtc,
    };
  }

  private static (int Year, int Month) ParseInitialPeriod(string value)
  {
    if (DateOnly.TryParseExact(
      value + "-01",
      "yyyy-MM-dd",
      CultureInfo.InvariantCulture,
      DateTimeStyles.None,
      out DateOnly date))
    {
      return (date.Year, date.Month);
    }

    throw new InvalidOperationException(FinancialPeriodMessages.InvalidInitialPeriodSetting);
  }

  /// <summary>"Today" in South Africa, not in the server's (often UTC) time zone.</summary>
  private DateOnly GetLocalToday()
  {
    TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
    DateTimeOffset local = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), zone);
    return DateOnly.FromDateTime(local.DateTime);
  }
}

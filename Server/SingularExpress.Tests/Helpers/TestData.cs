using Microsoft.AspNetCore.Identity;
using SingularExpress.Api.Models;
using SingularExpress.Api.Utils;

namespace SingularExpress.Tests.Helpers;

/// <summary>
/// Builders for test objects. Only made-up data: never use a real person's name, email,
/// ID number or password in tests (POPIA/GDPR).
/// </summary>
public static class TestData
{
  public const string Email = "test.user@singular.co.za";
  public const string Password = "Correct-Horse-1!";
  public const string OtherStrongPassword = "Brand-New-Pass-2@";

  /// <summary>13 October 2026, 10:00 in South Africa (08:00 UTC).</summary>
  public static readonly DateTimeOffset Now = new(2026, 10, 13, 8, 0, 0, TimeSpan.Zero);

  private static readonly PasswordHasher<User> Hasher = new();

  public static User User(UserRole role = UserRole.NormalUser, int id = 1, string password = Password)
  {
    User user = new()
    {
      Id = id,
      Email = Email,
      Role = role,
      IsActive = true,
      CreatedAtUtc = Now.UtcDateTime.AddYears(-1),
    };
    user.PasswordHash = Hasher.HashPassword(user, password);
    return user;
  }

  public static string HashPassword(string password)
  {
    return Hasher.HashPassword(new User(), password);
  }

  /// <summary>A stored period for the given calendar month, e.g. Period(2026, 10) = 2026/27 P07.</summary>
  public static FinancialPeriod Period(int calendarYear, int calendarMonth, int id = 1, PeriodStatus status = PeriodStatus.Open)
  {
    int fy = FinancialYearHelper.GetFinancialYearStart(calendarYear, calendarMonth);
    int number = FinancialYearHelper.GetPeriodNumber(calendarMonth);
    return new FinancialPeriod
    {
      Id = id,
      FinancialYearStart = fy,
      PeriodNumber = number,
      CalendarYear = calendarYear,
      CalendarMonth = calendarMonth,
      StartDate = FinancialYearHelper.GetStartDate(fy, number),
      EndDate = FinancialYearHelper.GetEndDate(fy, number),
      Status = status,
      OpenedAtUtc = Now.UtcDateTime.AddDays(-12),
    };
  }
}

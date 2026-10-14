using System.Globalization;

namespace SingularExpress.Api.Utils;

/// <summary>
/// Pure date maths for a financial year that runs from April to March.
/// No database, no clock: every method gives the same answer for the same input,
/// which makes it easy to unit test (see FinancialYearHelperTests).
/// </summary>
/// <example>
/// October 2026 → FinancialYearStart 2026, PeriodNumber 7, label "2026/27".
/// March 2027 → FinancialYearStart 2026, PeriodNumber 12.
/// April 2027 → FinancialYearStart 2027, PeriodNumber 1.
/// </example>
public static class FinancialYearHelper
{
  /// <summary>The calendar month in which the financial year starts.</summary>
  public const int FirstMonthOfYear = 4;

  public const int PeriodsPerYear = 12;

  /// <summary>Returns the calendar year in which the financial year containing this month starts.</summary>
  public static int GetFinancialYearStart(int calendarYear, int calendarMonth)
  {
    EnsureValidMonth(calendarMonth);
    return calendarMonth >= FirstMonthOfYear ? calendarYear : calendarYear - 1;
  }

  /// <summary>April → 1, May → 2 … December → 9, January → 10 … March → 12.</summary>
  public static int GetPeriodNumber(int calendarMonth)
  {
    EnsureValidMonth(calendarMonth);
    return ((calendarMonth - FirstMonthOfYear + PeriodsPerYear) % PeriodsPerYear) + 1;
  }

  /// <summary>Reverse of <see cref="GetPeriodNumber"/>: 1 → April … 12 → March.</summary>
  public static int GetCalendarMonth(int periodNumber)
  {
    EnsureValidPeriod(periodNumber);
    return ((periodNumber + FirstMonthOfYear - 2) % PeriodsPerYear) + 1;
  }

  /// <summary>Periods 1-9 (Apr-Dec) fall in the start year; 10-12 (Jan-Mar) in the next year.</summary>
  public static int GetCalendarYear(int financialYearStart, int periodNumber)
  {
    EnsureValidPeriod(periodNumber);
    return GetCalendarMonth(periodNumber) >= FirstMonthOfYear
      ? financialYearStart
      : financialYearStart + 1;
  }

  /// <summary>The period after the given one. Period 12 rolls into period 1 of the next year.</summary>
  public static (int FinancialYearStart, int PeriodNumber) GetNextPeriod(
    int financialYearStart,
    int periodNumber)
  {
    EnsureValidPeriod(periodNumber);
    return periodNumber == PeriodsPerYear
      ? (financialYearStart + 1, 1)
      : (financialYearStart, periodNumber + 1);
  }

  public static DateOnly GetStartDate(int financialYearStart, int periodNumber)
  {
    return new DateOnly(
      GetCalendarYear(financialYearStart, periodNumber),
      GetCalendarMonth(periodNumber),
      1);
  }

  /// <summary>Last day of the month (handles February and leap years).</summary>
  public static DateOnly GetEndDate(int financialYearStart, int periodNumber)
  {
    DateOnly start = GetStartDate(financialYearStart, periodNumber);
    return start.AddMonths(1).AddDays(-1);
  }

  /// <summary>2026 → "2026/27".</summary>
  public static string FormatFinancialYear(int financialYearStart)
  {
    int endYearShort = (financialYearStart + 1) % 100;
    return string.Create(
      CultureInfo.InvariantCulture,
      $"{financialYearStart}/{endYearShort:D2}");
  }

  /// <summary>(2026, 7) → "2026/27 P07 (October 2026)".</summary>
  public static string FormatPeriodLabel(int financialYearStart, int periodNumber)
  {
    string monthName = GetMonthName(GetCalendarMonth(periodNumber));
    int calendarYear = GetCalendarYear(financialYearStart, periodNumber);
    return string.Create(
      CultureInfo.InvariantCulture,
      $"{FormatFinancialYear(financialYearStart)} P{periodNumber:D2} ({monthName} {calendarYear})");
  }

  /// <summary>English month name, independent of the server's culture settings.</summary>
  public static string GetMonthName(int calendarMonth)
  {
    EnsureValidMonth(calendarMonth);
    return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(calendarMonth);
  }

  private static void EnsureValidMonth(int calendarMonth)
  {
    if (calendarMonth is < 1 or > 12)
    {
      throw new ArgumentOutOfRangeException(nameof(calendarMonth), "Month must be 1-12.");
    }
  }

  private static void EnsureValidPeriod(int periodNumber)
  {
    if (periodNumber is < 1 or > PeriodsPerYear)
    {
      throw new ArgumentOutOfRangeException(nameof(periodNumber), "Period must be 1-12.");
    }
  }
}

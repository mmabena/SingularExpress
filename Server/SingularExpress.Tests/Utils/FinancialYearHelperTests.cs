using SingularExpress.Api.Utils;

namespace SingularExpress.Tests.Utils;

/// <summary>Unit tests for <see cref="FinancialYearHelper"/>: the April-March calendar maths.</summary>
public class FinancialYearHelperTests
{
  [Theory]
  [InlineData(4, 1)]
  [InlineData(5, 2)]
  [InlineData(9, 6)]
  [InlineData(10, 7)]
  [InlineData(12, 9)]
  [InlineData(1, 10)]
  [InlineData(2, 11)]
  [InlineData(3, 12)]
  public void GetPeriodNumber_MapsCalendarMonthToPeriod(int month, int expectedPeriod)
  {
    Assert.Equal(expectedPeriod, FinancialYearHelper.GetPeriodNumber(month));
  }

  [Fact]
  public void GetCalendarMonth_IsTheReverseOfGetPeriodNumber()
  {
    for (int month = 1; month <= 12; month++)
    {
      int period = FinancialYearHelper.GetPeriodNumber(month);
      Assert.Equal(month, FinancialYearHelper.GetCalendarMonth(period));
    }
  }

  [Theory]
  [InlineData(2026, 10, 2026)]
  [InlineData(2026, 4, 2026)]
  [InlineData(2027, 3, 2026)]
  [InlineData(2027, 1, 2026)]
  [InlineData(2027, 4, 2027)]
  public void GetFinancialYearStart_ReturnsYearInWhichAprilFalls(int year, int month, int expected)
  {
    Assert.Equal(expected, FinancialYearHelper.GetFinancialYearStart(year, month));
  }

  [Theory]
  [InlineData(1, 2026)]
  [InlineData(9, 2026)]
  [InlineData(10, 2027)]
  [InlineData(12, 2027)]
  public void GetCalendarYear_JanuaryToMarchFallInTheFollowingYear(int period, int expected)
  {
    Assert.Equal(expected, FinancialYearHelper.GetCalendarYear(2026, period));
  }

  [Theory]
  [InlineData(2026, 7, 2026, 8)]
  [InlineData(2026, 9, 2026, 10)]
  [InlineData(2026, 12, 2027, 1)]
  public void GetNextPeriod_MovesForwardAndRollsMarchIntoApril(int fy, int period, int nextFy, int nextPeriod)
  {
    Assert.Equal((nextFy, nextPeriod), FinancialYearHelper.GetNextPeriod(fy, period));
  }

  [Theory]
  [InlineData(2026, 11, 2027, 2, 28)]
  [InlineData(2027, 11, 2028, 2, 29)]
  [InlineData(2026, 7, 2026, 10, 31)]
  public void GetEndDate_ReturnsLastDayOfMonthIncludingLeapYears(int fy, int period, int y, int m, int d)
  {
    Assert.Equal(new DateOnly(y, m, d), FinancialYearHelper.GetEndDate(fy, period));
  }

  [Theory]
  [InlineData(2026, "2026/27")]
  [InlineData(2099, "2099/00")]
  public void FormatFinancialYear_UsesStartYearAndTwoDigitEndYear(int fy, string expected)
  {
    Assert.Equal(expected, FinancialYearHelper.FormatFinancialYear(fy));
  }

  [Fact]
  public void FormatPeriodLabel_ForOctober2026()
  {
    Assert.Equal("2026/27 P07 (October 2026)", FinancialYearHelper.FormatPeriodLabel(2026, 7));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(13)]
  public void InvalidMonthOrPeriod_Throws(int value)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => FinancialYearHelper.GetPeriodNumber(value));
    Assert.Throws<ArgumentOutOfRangeException>(() => FinancialYearHelper.GetCalendarMonth(value));
  }
}

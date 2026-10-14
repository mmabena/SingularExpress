using SingularExpress.Api.DTOs.FinancialPeriod;
using SingularExpress.Api.Models;
using SingularExpress.Api.Utils;

namespace SingularExpress.Api.Mappers;

/// <summary>Converts <see cref="FinancialPeriod"/> entities to DTOs. Mappers never touch the database.</summary>
public static class FinancialPeriodMapper
{
  public static FinancialPeriodDto ToDto(this FinancialPeriod period)
  {
    return new FinancialPeriodDto
    {
      Id = period.Id,
      FinancialYear = FinancialYearHelper.FormatFinancialYear(period.FinancialYearStart),
      FinancialYearStart = period.FinancialYearStart,
      PeriodNumber = period.PeriodNumber,
      Label = FinancialYearHelper.FormatPeriodLabel(period.FinancialYearStart, period.PeriodNumber),
      CalendarYear = period.CalendarYear,
      CalendarMonth = period.CalendarMonth,
      MonthName = FinancialYearHelper.GetMonthName(period.CalendarMonth),
      StartDate = period.StartDate,
      EndDate = period.EndDate,
      Status = period.Status.ToString(),
      ClosedAtUtc = period.ClosedAtUtc,
    };
  }
}

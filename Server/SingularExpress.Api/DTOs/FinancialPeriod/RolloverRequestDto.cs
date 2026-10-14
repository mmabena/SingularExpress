using System.ComponentModel.DataAnnotations;
using SingularExpress.Api.Constants.Messages;

namespace SingularExpress.Api.DTOs.FinancialPeriod;

/// <summary>
/// Body of POST /api/financial-periods/rollover.
/// The Client sends the id of the period it believes is open. If that is no longer true
/// (double click, or another SuperUser got there first) the rollover is refused, so a
/// month can never be skipped by accident.
/// </summary>
public class RolloverRequestDto
{
  [Required(ErrorMessage = FinancialPeriodMessages.ExpectedPeriodRequired)]
  [Range(1, int.MaxValue, ErrorMessage = FinancialPeriodMessages.ExpectedPeriodRequired)]
  public int ExpectedCurrentPeriodId { get; set; }
}

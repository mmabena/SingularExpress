using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingularExpress.Api.Constants;
using SingularExpress.Api.DTOs;
using SingularExpress.Api.DTOs.FinancialPeriod;
using SingularExpress.Api.Extensions;
using SingularExpress.Api.Interfaces;

namespace SingularExpress.Api.Controllers;

/// <summary>
/// Financial periods (April-March). Any signed-in user can read them;
/// only a SuperUser can roll over to the next month.
/// </summary>
[ApiController]
[Route("api/financial-periods")]
[Authorize(Policy = Policies.SuperOrNormalUser)]
public class FinancialPeriodController : ControllerBase
{
  private readonly IFinancialPeriodService _periodService;

  public FinancialPeriodController(IFinancialPeriodService periodService)
  {
    _periodService = periodService;
  }

  /// <summary>The period that is currently open.</summary>
  [HttpGet("current")]
  [ProducesResponseType<FinancialPeriodDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<FinancialPeriodDto>> GetCurrent()
  {
    return Ok(await _periodService.GetCurrentPeriodAsync());
  }

  /// <summary>All 12 months of a financial year, e.g. /api/financial-periods/years/2026 for 2026/27.</summary>
  [HttpGet("years/{financialYearStart:int}")]
  [ProducesResponseType<FinancialYearOverviewDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<FinancialYearOverviewDto>> GetYear(int financialYearStart)
  {
    return Ok(await _periodService.GetFinancialYearOverviewAsync(financialYearStart));
  }

  /// <summary>
  /// SUPERUSER ONLY. Closes the open period and opens the next one. Cannot be undone.
  /// Send the id of the period you are closing as ExpectedCurrentPeriodId.
  /// </summary>
  [HttpPost("rollover")]
  [Authorize(Policy = Policies.SuperUserOnly)]
  [ProducesResponseType<RolloverResultDto>(StatusCodes.Status200OK)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
  [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<RolloverResultDto>> Rollover([FromBody] RolloverRequestDto request)
  {
    RolloverResultDto result = await _periodService.RolloverAsync(request, User.GetUserId());
    return Ok(result);
  }
}

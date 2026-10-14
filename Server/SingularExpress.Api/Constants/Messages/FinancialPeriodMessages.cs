namespace SingularExpress.Api.Constants.Messages;

/// <summary>Messages for financial periods and the month-end rollover.</summary>
public static class FinancialPeriodMessages
{
  public const string OnlySuperUserCanRollover = "Only a SuperUser can roll over the financial period.";
  public const string NoOpenPeriod = "There is no open financial period. Contact a SuperUser.";
  public const string ExpectedPeriodRequired = "The id of the period being closed is required.";

  /// <summary>
  /// Returned when the Client sends an old period id, for example after a double click
  /// or when another SuperUser rolled over first. Stops the system skipping a month.
  /// </summary>
  public const string PeriodChangedSinceLoaded =
    "The open period has changed since you loaded this page. Refresh and try again.";

  /// <summary>Format with the period end date, e.g. string.Format(..., "31 Oct 2026").</summary>
  public const string PeriodNotEnded =
    "This period only ends on {0}. Rollover before the period end is disabled.";

  public const string NextPeriodAlreadyExists =
    "The next period already exists. Refresh to see the current state.";
  public const string ConcurrentRollover =
    "Another rollover was saved at the same time. Refresh to see the current state.";
  public const string InvalidFinancialYear = "Financial year must be between 2000 and 2100.";
  public const string InvalidInitialPeriodSetting =
    "FinancialPeriod:InitialPeriod must use the format yyyy-MM, e.g. 2026-10.";

  /// <summary>Format with closed and opened period labels.</summary>
  public const string RolloverSuccessful = "Closed {0}. {1} is now open.";
}

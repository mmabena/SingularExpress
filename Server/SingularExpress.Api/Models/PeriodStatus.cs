namespace SingularExpress.Api.Models;

/// <summary>
/// Open: the current working month. Exactly one period is open at any time.
/// Closed: rolled over. Closed periods are never re-opened.
/// </summary>
public enum PeriodStatus
{
  Open = 0,
  Closed = 1,
}

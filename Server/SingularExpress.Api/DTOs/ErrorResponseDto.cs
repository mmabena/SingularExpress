namespace SingularExpress.Api.DTOs;

/// <summary>
/// The only error shape the API returns: <c>{ "errors": ["..."], "traceId": "..." }</c>.
/// Same "errors" array as HRConnect, so Client code can always read <c>data.errors</c>.
/// </summary>
public class ErrorResponseDto
{
  public IReadOnlyList<string> Errors { get; set; } = [];

  /// <summary>Correlates a user's report with the server log entry. Contains no personal data.</summary>
  public string? TraceId { get; set; }
}

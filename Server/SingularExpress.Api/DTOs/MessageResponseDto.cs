namespace SingularExpress.Api.DTOs;

/// <summary>Success response that only carries a message: <c>{ "message": "..." }</c>.</summary>
public class MessageResponseDto
{
  public MessageResponseDto(string message)
  {
    Message = message;
  }

  public string Message { get; }
}

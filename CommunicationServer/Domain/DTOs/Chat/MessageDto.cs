namespace Domain.DTOs.Chat;

public class MessageDto
{
    public string Username { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public ulong GameRoom { get; init; }
}
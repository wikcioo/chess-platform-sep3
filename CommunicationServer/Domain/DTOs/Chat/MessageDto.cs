namespace Domain.DTOs.Chat;

public class MessageDto
{
    public string Username { get; init; }
    public string Body { get; init; }
    public string Receiver { get; init; }
}
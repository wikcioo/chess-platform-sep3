namespace Domain.DTOs.Draw;

public class RequestDrawDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
}
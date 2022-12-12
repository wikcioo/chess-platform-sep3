namespace Domain.DTOs.Resignation;

public class RequestResignDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
}
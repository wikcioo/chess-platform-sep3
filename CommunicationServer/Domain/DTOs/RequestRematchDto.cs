namespace Domain.DTOs;

public class RequestRematchDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
}
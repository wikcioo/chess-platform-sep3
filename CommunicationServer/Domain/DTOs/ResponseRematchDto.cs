namespace Domain.DTOs;

public class ResponseRematchDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool Accept { get; set; }
}
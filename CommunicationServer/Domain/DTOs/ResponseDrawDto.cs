namespace Domain.DTOs;

public class ResponseDrawDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool Accept { get; set; }
}
namespace Domain.DTOs.Game;

public class RequestJoinGameDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
}
namespace Domain.DTOs;

public class GameRoomDto
{
    public ulong GameRoom { get; set; }
    public string Creator { get; set; } = string.Empty;
    public string UsernameWhite { get; set; } = string.Empty;
    public string UsernameBlack { get; set; } = string.Empty;
    public uint Seconds { get; set; }
    public uint Increment { get; set; }
}
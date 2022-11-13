using Domain.Enums;

namespace Domain.DTOs.GameRoomData;

public class JoinableGameRoomDataDto
{
    public ulong GameRoom { get; set; }
    public string Username { get; set; } = string.Empty;
    public uint Seconds { get; set; }
    public uint Increment { get; set; }
    public GameSides Side { get; set; }
}
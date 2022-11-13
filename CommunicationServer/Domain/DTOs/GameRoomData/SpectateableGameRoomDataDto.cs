namespace Domain.DTOs.GameRoomData;

public class SpectateableGameRoomDataDto
{
    public ulong GameRoom { get; set; }
    public string UsernameWhite { get; set; } = string.Empty;
    public string UsernameBlack { get; set; } = string.Empty;
    public uint Seconds { get; set; }
    public uint Increment { get; set; }
}
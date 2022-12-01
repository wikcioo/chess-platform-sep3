namespace Domain.DTOs.GameEvents;

public class GameRoomEventDto
{
    public GameEventDto? GameEventDto { get; set; }
    public ulong GameRoomId { get; set; }
}
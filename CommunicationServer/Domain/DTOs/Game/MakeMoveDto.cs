namespace Domain.DTOs.Game;

public class MakeMoveDto
{
    public ulong GameRoom { get; set; }
    public string FromSquare { get; set; } = string.Empty;
    public string ToSquare { get; set; } = string.Empty;
    public uint? MoveType { get; set; }
    public uint? Promotion { get; set; }
    public string Username {get; set; } = string.Empty;
}
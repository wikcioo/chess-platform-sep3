namespace Domain.DTOs;

public class MakeMoveDto
{
    public ulong GameRoom { get; set; }
    public string FromSquare { get; set; }
    public string ToSquare { get; set; }
    public uint? MoveType { get; set; }
    public uint? Promotion { get; set; }
    public string Username {get; set; }
}
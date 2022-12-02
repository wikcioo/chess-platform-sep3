namespace Domain.DTOs;

public class ResponseGameDto
{
    public bool Success { get; set; }
    public ulong GameRoom { get; set; }
    public string Opponent { get; set; } = string.Empty;
    public string Fen { get; set; } = string.Empty;
    public bool IsWhite { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
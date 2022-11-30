using Domain.Enums;

namespace Domain.DTOs.GameEvents;

public class GameEventDto
{
    public string FenString { get; set; } = string.Empty;
    public uint GameEndType { get; set; }
    public double TimeLeftMs { get; set; }
    public bool IsWhite { get; set; }
    public GameStreamEvents Event { get; set; }
    public string UsernameWhite { get; set; } = string.Empty;
    public string UsernameBlack { get; set; } = string.Empty;
}
using Domain.Enums;

namespace Domain.DTOs;

public class GameCreationDto
{
    public string Creator { get; set; }
    public string? PlayerWhite { get; set; }
    public string? PlayerBlack { get; set; }
    public OpponentTypes GameType { get; set; }
    public  GameOutcome GameOutcome { get; set; }

    public bool IsVisible { get; set; }

    public uint TimeControlDurationSeconds { get; set; }
    public uint TimeControlIncrementSeconds { get; set; }
}
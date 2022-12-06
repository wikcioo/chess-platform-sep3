using Domain.Enums;

namespace Domain.Models;

public class GameRoom
{
    public string Creator { get; }
    public string? PlayerWhite { get; set; }
    public string? PlayerBlack { get; set; }
    public OpponentTypes GameType { get; }

    public GameSides GameSide { get; }
    public bool IsVisible { get; set; }

    public uint TimeControlDurationSeconds { get; }
    public uint TimeControlIncrementSeconds { get; }

    public GameRoom(string creator, OpponentTypes gameType, uint timeControlDurationSeconds,
        uint timeControlIncrementSeconds, GameSides gameSide, bool isVisible)
    {
        Creator = creator;
        GameType = gameType;
        TimeControlDurationSeconds = timeControlDurationSeconds;
        TimeControlIncrementSeconds = timeControlIncrementSeconds;
        GameSide = gameSide;
        IsVisible = isVisible;
    }
}
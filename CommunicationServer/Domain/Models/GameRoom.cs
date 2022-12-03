using Domain.Enums;

namespace Domain.Models;

public class GameRoom
{
    public string Creator { get; }
    public string? PlayerWhite { get; set; }
    public string? PlayerBlack { get; set; }
    public OpponentTypes GameType { get; }

    public readonly uint TimeControlDurationSeconds;
    public readonly uint TimeControlIncrementSeconds;

    public GameRoom(string creator, OpponentTypes gameType, uint timeControlDurationSeconds,
        uint timeControlIncrementSeconds)
    {
        Creator = creator;
        GameType = gameType;
        TimeControlDurationSeconds = timeControlDurationSeconds;
        TimeControlIncrementSeconds = timeControlIncrementSeconds;
    }
}
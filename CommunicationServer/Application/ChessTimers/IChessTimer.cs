using Domain.DTOs.GameEvents;

namespace Application.ChessTimers;

public interface IChessTimer
{
    uint TimeControlDurationMs { get; }
    uint TimeControlIncrementMs { get; }
    double WhiteRemainingTimeMs { get; }
    double BlackRemainingTimeMs { get; }

    event GameEventHandler Elapsed;
    void StartTimers(bool both = true, bool white = true);
    void StopTimers(bool both = true, bool white = true);
    void UpdateTimers(bool whitePlaying);
}
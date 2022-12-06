using Domain.DTOs.GameEvents;

namespace Application.ChessTimers;

public interface IChessTimer
{
    public uint TimeControlDurationMs { get; }
    public uint TimeControlIncrementMs { get; }
    public double WhiteRemainingTimeMs { get; }
    public double BlackRemainingTimeMs { get; }    
    
    public event GameEventHandler Elapsed;
    void StartTimers(bool both = true, bool white = true);
    void StopTimers(bool both = true, bool white = true);
    void UpdateTimers(bool whitePlaying);

}
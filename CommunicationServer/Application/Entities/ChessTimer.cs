using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using Rudzoft.ChessLib.Enums;

namespace Application.Entities;

public class ChessTimer
{
    private readonly PausableTimer _whiteTimer = new(1000.0);
    private readonly PausableTimer _blackTimer = new(1000.0);
    public readonly uint TimeControlDurationMs;
    public readonly uint TimeControlIncrementMs;
    private bool _whitePlaying;
    public double WhiteRemainingTimeMs { get; private set; }
    public double BlackRemainingTimeMs { get; private set; }
    
    public delegate void EventHandler(object sender, EventArgs args, GameEventDto dto);
    public event EventHandler ThrowEvent = delegate{};

    public ChessTimer(bool whitePlaying, uint timeControlDurationSeconds, uint timeControlIncrementSeconds)
    {
        _whitePlaying = whitePlaying;
        TimeControlDurationMs = timeControlDurationSeconds * 1000;
        TimeControlIncrementMs = timeControlIncrementSeconds * 1000;

        WhiteRemainingTimeMs = TimeControlDurationMs;
        BlackRemainingTimeMs = TimeControlDurationMs;
        
        _whiteTimer.Elapsed += OnOneSecElapsed;
        _blackTimer.Elapsed += OnOneSecElapsed;
    }

    public void StartTimers(bool both = true, bool white = true)
    {
        if (both)
        {
            _whiteTimer.Start();
            _blackTimer.Start();
        }
        else
        {
            if (white) _whiteTimer.Start();
            else _blackTimer.Start();
        }
    }

    public void StopTimers(bool both = true, bool white = true)
    {
        if (both)
        {
            _whiteTimer.Stop();
            _blackTimer.Stop();
        }
        else
        {
            if (white) _whiteTimer.Stop();
            else _blackTimer.Stop();
        }
    }
    
    public void UpdateTimers(bool whitePlaying)
    {
        if (whitePlaying)
        {
            _whitePlaying = false;
            WhiteRemainingTimeMs += TimeControlIncrementMs;
            WhiteRemainingTimeMs -= _whiteTimer.Pause();
            _blackTimer.Resume();
        }
        else
        {
            _whitePlaying = true;
            BlackRemainingTimeMs += TimeControlIncrementMs;
            BlackRemainingTimeMs -= _blackTimer.Pause();
            _whiteTimer.Resume();
        }
    }

    private void OnOneSecElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_whitePlaying)
            WhiteRemainingTimeMs -= 1000;
        else
            BlackRemainingTimeMs -= 1000;

        if (WhiteRemainingTimeMs <= 0 || BlackRemainingTimeMs <= 0)
        {
            StopTimers();
            ThrowEvent(this, EventArgs.Empty, new GameEventDto()
            {
                Event = GameStreamEvents.TimeUpdate,
                IsWhite = _whitePlaying,
                TimeLeftMs = WhiteRemainingTimeMs <= 0 ? BlackRemainingTimeMs : WhiteRemainingTimeMs,
                GameEndType = (uint)GameEndTypes.TimeIsUp
            });
        }
        else
        {
            ThrowEvent(this, EventArgs.Empty, new GameEventDto()
            {
                Event = GameStreamEvents.TimeUpdate,
                IsWhite = _whitePlaying,
                TimeLeftMs = _whitePlaying ? WhiteRemainingTimeMs : BlackRemainingTimeMs,
                GameEndType = (uint)GameEndTypes.None
            });
        }
    }
}
using Application.Utils;
using Domain.DTOs;
using Rudzoft.ChessLib.Enums;

namespace Application.Entities;

public class ChessTimer
{
    private readonly PausableTimer _whiteTimer = new(1000.0);
    private readonly PausableTimer _blackTimer = new(1000.0);
    private readonly uint _timeControlSeconds;
    private readonly uint _timeControlIncrement;
    private readonly ValueWrapper<bool> _whitePlaying;
    public double WhiteRemainingTimeMs { get; private set; }
    public double BlackRemainingTimeMs { get; private set; }
    
    public delegate void EventHandler(object sender, EventArgs args, ResponseJoinedGameDto dto);
    public event EventHandler ThrowEvent = delegate{};

    public ChessTimer(ref ValueWrapper<bool> whitePlaying, uint timeControlSeconds, uint timeControlIncrement)
    {
        _whitePlaying = whitePlaying;
        
        _timeControlSeconds = timeControlSeconds;
        _timeControlIncrement = timeControlIncrement;

        WhiteRemainingTimeMs = timeControlSeconds * 1000;
        BlackRemainingTimeMs = timeControlSeconds * 1000;
        
        _whiteTimer.Elapsed += OnOneSecElapsed;
        _blackTimer.Elapsed += OnOneSecElapsed;
    }

    public void StartTimers()
    {
        _whiteTimer.Start();
        _blackTimer.Start();
    }
    
    public void UpdateTimers()
    {
        if (_whitePlaying.Value)
        {
            _whitePlaying.Value = false;
            WhiteRemainingTimeMs += _timeControlIncrement * 1000;
            WhiteRemainingTimeMs -= _whiteTimer.Pause();
            _blackTimer.Resume();
        }
        else
        {
            _whitePlaying.Value = true;
            BlackRemainingTimeMs += _timeControlIncrement * 1000;
            BlackRemainingTimeMs -= _blackTimer.Pause();
            _whiteTimer.Resume();
        }
    }

    private void OnOneSecElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_whitePlaying.Value)
            WhiteRemainingTimeMs -= 1000;
        else
            BlackRemainingTimeMs -= 1000;

        if (WhiteRemainingTimeMs <= 0 || BlackRemainingTimeMs <= 0)
        {
            StopTimers();
            ThrowEvent(this, EventArgs.Empty, new ResponseJoinedGameDto()
            {
                FenString = "",
                IsWhite = _whitePlaying.Value,
                TimeLeftMs = WhiteRemainingTimeMs <= 0 ? BlackRemainingTimeMs : WhiteRemainingTimeMs,
                GameEndType = (uint)GameEndTypes.TimeIsUp
            });
        }
        else
        {
            ThrowEvent(this, EventArgs.Empty, new ResponseJoinedGameDto()
            {
                FenString = "",
                IsWhite = _whitePlaying.Value,
                TimeLeftMs = _whitePlaying.Value ? WhiteRemainingTimeMs : BlackRemainingTimeMs,
                GameEndType = (uint)GameEndTypes.None
            });
        }
    }

    private void StopTimers()
    {
        _whiteTimer.Stop();
        _blackTimer.Stop();
    }
}
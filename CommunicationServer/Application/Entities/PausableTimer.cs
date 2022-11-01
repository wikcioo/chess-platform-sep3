using System.Diagnostics;

namespace Application.Entities;

public class PausableTimer : System.Timers.Timer
{
    private readonly Stopwatch _stopwatch;
    private readonly double _initialIntervalMs;
    private double _timeRemainingAfterPauseMs;
    private bool _wasResumed;

    public PausableTimer(double interval = 100.0) : base(interval)
    {
        _initialIntervalMs = interval;
        _stopwatch = new Stopwatch();
        
        Elapsed += OnTimeElapsedEvent;
    }

    public new void Start()
    {
        ResetStopwatch();
        base.Start();
    }

    public double Pause()
    {
        Stop();
        _stopwatch.Stop();
        _timeRemainingAfterPauseMs = Interval - _stopwatch.Elapsed.TotalMilliseconds;
        return _timeRemainingAfterPauseMs;
    }

    public void Resume()
    {
        _wasResumed = true;
        Interval = _timeRemainingAfterPauseMs == 0 ? _initialIntervalMs : _timeRemainingAfterPauseMs;
        _timeRemainingAfterPauseMs = 0.0;
        Start();
    }

    private void OnTimeElapsedEvent(object? source, System.Timers.ElapsedEventArgs e)
    {
        if (_wasResumed)
        {
            _wasResumed = false;
            Stop();
            Interval = _initialIntervalMs;
            Start();
        }
        
        ResetStopwatch();
    }

    private void ResetStopwatch()
    {
        _stopwatch.Reset();
        _stopwatch.Start();
    }
}
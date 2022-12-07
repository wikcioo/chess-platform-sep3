namespace Application.GameRoomHandlers;

public class CountDownTimer
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public void StopTimer()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task<bool> StartTimer(int timeInSeconds)
    {
        bool timerStopped = false;

        var timer = new System.Timers.Timer(timeInSeconds * 1000);

        timer.Elapsed += (_, _) =>
        {
            timer.Stop();
            timerStopped = true;
        };

        timer.Start();

        try
        {
            await Task.Delay(timeInSeconds * 1000, _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            timerStopped = true;
        }

        return timerStopped;
    }
}
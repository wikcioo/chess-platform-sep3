namespace Application.GameRoomHandlers;

public class CountDownTimer
{
    private CancellationTokenSource _cancellationTokenSource = new();

    public void StopTimer()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task<bool> StartTimer(int timeInSeconds)
    {
        _cancellationTokenSource = new();
        bool timerStopped = false;

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
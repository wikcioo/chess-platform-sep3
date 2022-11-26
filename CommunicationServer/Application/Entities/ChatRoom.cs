using System.Reactive.Linq;
using Domain.DTOs.Chat;

namespace Application.Entities;

public class ChatRoom
{
    public event Action<MessageDto> MessageReceived;
    private List<MessageDto> _logs = new();


    public void Add(MessageDto message)
    {
        _logs.Add(message);
        MessageReceived?.Invoke(message);
    }

    public IObservable<MessageDto> GetMessagedAsObservable()
    {
        var newLogs = Observable.FromEvent<MessageDto>(
            (x) => MessageReceived += x,
            (x) => MessageReceived -= x);
        return _logs.ToObservable().Concat(newLogs);
    }

    public List<MessageDto> GetLog()
    {
        return _logs;
    }
}
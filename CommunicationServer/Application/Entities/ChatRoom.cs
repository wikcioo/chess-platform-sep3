using System.Reactive.Linq;
using Domain.DTOs.Chat;

namespace Application.Entities;

public class ChatRoom
{
    private List<string> _users;

    public event Action<MessageDto> MessageReceived;
    private List<MessageDto> _logs = new();

    public ChatRoom(string user1, string user2)
    {
        _users = new List<string>
        {
            user1,
            user2
        };
    }

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
}
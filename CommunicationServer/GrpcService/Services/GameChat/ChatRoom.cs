using System.Reactive.Linq;

namespace GrpcService.Services;

public class ChatRoom
{
    public List<string> users;
    
    public event Action<Message> MessageReceived;
    private List<Message> _logs = new();

    public ChatRoom(string user1,string user2)
    {
        users = new List<string>
        {
            user1,
            user2
        };
    }

    public void Add(Message message)
    {
        _logs.Add(message);
        MessageReceived?.Invoke(message);
    }
    
    
    public IObservable<Message> GetMessagedAsObservable()
    {
        var newLogs = Observable.FromEvent<Message>(
            (x) => MessageReceived+=x,
            (x) => MessageReceived-=x);
        return _logs.ToObservable().Concat(newLogs);
    }
}
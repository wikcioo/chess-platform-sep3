using Domain.DTOs.Chat;

namespace Application.Entities;

public class ChatRoom
{
    public event Action<MessageDto>? MessageReceived;
    private List<MessageDto> _logs = new();


    public void Add(MessageDto message)
    {
        _logs.Add(message);
        MessageReceived?.Invoke(message);
    }


    public List<MessageDto> GetLog()
    {
        return _logs;
    }
}
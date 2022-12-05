using Domain.DTOs.Chat;

namespace Domain.Models;

public class ChatRoom
{
    private List<MessageDto> _logs = new();

    public void Add(MessageDto message)
    {
        _logs.Add(message);
    }


    public List<MessageDto> GetLog()
    {
        return _logs;
    }
}
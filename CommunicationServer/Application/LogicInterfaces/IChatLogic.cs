using Domain.DTOs.Chat;

namespace Application.LogicInterfaces;

public interface IChatLogic
{
    public void Add(MessageDto message);
    public void StartChatRoom(ulong key);
    public List<MessageDto> GetLog(ulong gameRoom);
}
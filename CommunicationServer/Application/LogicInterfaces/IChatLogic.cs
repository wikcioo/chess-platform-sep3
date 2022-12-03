using Domain.DTOs.Chat;

namespace Application.LogicInterfaces;

public interface IChatLogic
{
    void Add(MessageDto message);
    void StartChatRoom(ulong key);
    List<MessageDto> GetLog(ulong gameRoom);
}
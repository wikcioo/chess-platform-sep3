using Domain.DTOs;
using Domain.DTOs.Chat;

namespace Application.LogicInterfaces;

public interface IChatLogic
{
    public void Add(MessageDto message);
    public IObservable<MessageDto> GetMessagesAsObservable(RequestMessageDto request);
    public void StartChatRoom(ulong key);
}
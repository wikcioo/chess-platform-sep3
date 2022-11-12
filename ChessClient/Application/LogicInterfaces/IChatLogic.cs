using Domain.DTOs.Chat;

namespace Application.LogicInterfaces;

public interface IChatLogic
{
    public event Action<MessageDto> MessageReceived;
    Task WriteMessageAsync(MessageDto dto);
    Task StartMessagingAsync(RequestMessageDto dto);
}
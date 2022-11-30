using Domain.DTOs.Chat;

namespace Application.LogicInterfaces;

public interface IChatLogic
{
    public event Action<string>? MessageReceived;
    Task WriteMessageAsync(MessageDto dto);
}
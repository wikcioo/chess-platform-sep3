using Domain.DTOs.Chat;

namespace HttpClients.ClientInterfaces;

public interface IChatService
{
    public event Action<string>? MessageReceived;
    Task WriteMessageAsync(MessageDto dto);
}
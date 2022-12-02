using Domain.DTOs.Chat;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IChatHub
{
    event Action<MessageDto>? MessageReceived;
    event Action<List<MessageDto>>? ChatLogReceived;
    Task WriteMessageAsync(MessageDto dto);
}
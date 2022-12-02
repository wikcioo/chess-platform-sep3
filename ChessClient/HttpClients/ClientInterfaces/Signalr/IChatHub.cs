using Domain.DTOs.Chat;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IChatHub
{
    public event Action<MessageDto>? MessageReceived;
    public event Action<List<MessageDto>>? ChatLogReceived;
    Task WriteMessageAsync(MessageDto dto);
}
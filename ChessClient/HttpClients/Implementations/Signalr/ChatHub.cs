using Domain.DTOs.Chat;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations.Signalr;

public class ChatHub : IChatHub
{
    private readonly IHubConnectionWrapper _hubConnectionWrapper;
    public event Action<MessageDto>? MessageReceived;
    public event Action<List<MessageDto>>? ChatLogReceived;

    public ChatHub(IHubConnectionWrapper hubConnectionWrapper)
    {
        _hubConnectionWrapper = hubConnectionWrapper;
        if (_hubConnectionWrapper.HubConnection is null) return;

        _hubConnectionWrapper.HubConnection.On<MessageDto>("ReceiveMessage",
            (dto) => { MessageReceived?.Invoke(dto); });
        _hubConnectionWrapper.HubConnection.On<List<MessageDto>>("GetLog",
            log => ChatLogReceived?.Invoke(log));
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        if (_hubConnectionWrapper.HubConnection is not null)
        {
            await _hubConnectionWrapper.HubConnection.SendAsync("SendMessage", dto);
        }
    }

}
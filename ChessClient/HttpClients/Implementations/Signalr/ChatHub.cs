using Domain.DTOs.Chat;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations.Signalr;

public class ChatHub : IChatHub
{
    private readonly IHubConnectionHandler _hubConnectionHandler;
    public event Action<MessageDto>? MessageReceived;
    public event Action<List<MessageDto>>? ChatLogReceived;

    public ChatHub(IHubConnectionHandler hubConnectionHandler)
    {
        _hubConnectionHandler = hubConnectionHandler;
        if (_hubConnectionHandler.HubConnection is null) return;

        _hubConnectionHandler.HubConnection.On<MessageDto>("ReceiveMessage",
            (dto) => { MessageReceived?.Invoke(dto); });
        _hubConnectionHandler.HubConnection.On<List<MessageDto>>("GetLog",
            log => ChatLogReceived?.Invoke(log));
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        if (_hubConnectionHandler.HubConnection is not null)
        {
            await _hubConnectionHandler.HubConnection.SendAsync("SendMessage", dto);
        }
    }

}
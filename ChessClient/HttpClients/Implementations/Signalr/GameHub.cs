using Domain.DTOs.GameEvents;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations.Signalr;

public class GameHub : IGameHub
{
    private readonly IHubConnectionWrapper _hubConnectionWrapper;
    public event Action<GameEventDto>? GameEventReceived;

    public GameHub(IHubConnectionWrapper hubConnectionWrapper)
    {
        _hubConnectionWrapper = hubConnectionWrapper;
    }

    public void StartListeningToGameEvents()
    {
        _hubConnectionWrapper.HubConnection?.Remove("GameStreamDto");
        _hubConnectionWrapper.HubConnection?.On<GameEventDto>("GameStreamDto",
            x => GameEventReceived?.Invoke(x));
    }

    public async Task LeaveRoom(ulong? gameRoomId)
    {
        await _hubConnectionWrapper.LeaveRoom(gameRoomId);
    }

    public async Task JoinRoom(ulong? gameRoomId)
    {
        await _hubConnectionWrapper.JoinRoom(gameRoomId);
    }
}
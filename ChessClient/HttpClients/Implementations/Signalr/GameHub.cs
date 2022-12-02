using Domain.DTOs.GameEvents;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations.Signalr;

public class GameHub : IGameHub
{
    private readonly IHubConnectionHandler _hubConnectionHandler;
    public event Action<GameEventDto>? GameEventReceived;

    public GameHub(IHubConnectionHandler hubConnectionHandler)
    {
        _hubConnectionHandler = hubConnectionHandler;
    }

    public void StartListeningToGameEvents()
    {
        _hubConnectionHandler.HubConnection?.Remove("GameStreamDto");
        _hubConnectionHandler.HubConnection?.On<GameEventDto>("GameStreamDto",
            x => GameEventReceived?.Invoke(x));
    }

    public async Task LeaveRoom(ulong? gameRoomId)
    {
        await _hubConnectionHandler.LeaveRoom(gameRoomId);
    }

    public async Task JoinRoom(ulong? gameRoomId)
    {
        await _hubConnectionHandler.JoinRoom(gameRoomId);
    }
}
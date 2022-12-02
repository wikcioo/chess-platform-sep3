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

    public async Task LeaveRoomAsync(ulong? gameRoomId)
    {
        await _hubConnectionHandler.LeaveRoomAsync(gameRoomId);
    }

    public async Task JoinRoomAsync(ulong? gameRoomId)
    {
        await _hubConnectionHandler.JoinRoomAsync(gameRoomId);
    }
}
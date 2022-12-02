using Domain.DTOs.AuthorizedUserEvents;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IHubConnectionHandler
{
    HubConnection? HubConnection { get; }
    event Action<AuthorizedUserEventDto>? NewGameOffer;
    Task StartHubConnection(IAuthService authService);
    Task LeaveRoom(ulong? gameRoomId);
    Task JoinRoom(ulong? gameRoomId);
    Task StopHubConnection();
    void JoinUserEvents();
}
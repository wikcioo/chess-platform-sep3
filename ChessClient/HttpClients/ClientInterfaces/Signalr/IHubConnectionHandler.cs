using Domain.DTOs.AuthorizedUserEvents;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IHubConnectionHandler
{
    HubConnection? HubConnection { get; }
    event Action<AuthorizedUserEventDto>? NewGameOffer;
    Task StartHubConnectionAsync(IAuthService authService);
    Task LeaveRoomAsync(ulong? gameRoomId);
    Task JoinRoomAsync(ulong? gameRoomId);
    Task StopHubConnectionAsync();
    void JoinUserEvents();
}
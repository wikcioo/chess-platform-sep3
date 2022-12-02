using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IHubConnectionWrapper
{
    public HubConnection? HubConnection { get; }
    Task StartHubConnection(IAuthService authService);
    Task LeaveRoom(ulong? gameRoomId);
    Task JoinRoom(ulong? gameRoomId);
    Task StopHubConnection();
}
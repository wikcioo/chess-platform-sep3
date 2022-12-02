using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IHubConnectionWrapper
{
    public HubConnection? HubConnection { get; }
    Task StartHubConnection(IAuthService authService);
    void LeaveRoom(ulong? gameRoomId);
    void JoinRoom(ulong? gameRoomId);
    Task StopHubConnection();
}
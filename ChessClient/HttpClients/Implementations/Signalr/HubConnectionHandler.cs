using Domain.DTOs.AuthorizedUserEvents;
using Domain.Enums;
using HttpClients.ClientInterfaces;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations.Signalr;

public class HubConnectionHandler : IHubConnectionHandler
{
    public HubConnection? HubConnection { get; set; }
    public event Action<AuthorizedUserEventDto>? NewGameOffer;

    public async Task StartHubConnection(IAuthService authService)
    {
        if (HubConnection is not null)
        {
            await HubConnection.DisposeAsync();
        }

        HubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7233/gamehub",
                options => { options.AccessTokenProvider = () => Task.FromResult(authService.GetJwtToken())!; })
            .WithAutomaticReconnect()
            .Build();
        //Required so the connection is not dropped
        HubConnection.On<string>("DummyConnection", _ => { });
        await HubConnection.StartAsync();
    }

    public async Task LeaveRoom(ulong? gameRoomId)
    {
        if (HubConnection is not null)
        {
            await HubConnection.SendAsync("LeaveRoom", gameRoomId);
        }
    }

    public async Task JoinRoom(ulong? gameRoomId)
    {
        if (HubConnection is not null)
        {
            await HubConnection.SendAsync("JoinRoom", gameRoomId);
        }
    }

    public async Task StopHubConnection()
    {
        if (HubConnection is not null)
        {
            await HubConnection.StopAsync();
            await HubConnection.DisposeAsync();
            HubConnection = null;
        }
    }

    public void JoinUserEvents()
    {
        HubConnection?.Remove("AuthorizedUserEventDto");
        HubConnection?.On<AuthorizedUserEventDto>("AuthorizedUserEventDto",
            ListenToAuthorizedUserEvents);
    }

    private void ListenToAuthorizedUserEvents(AuthorizedUserEventDto dto)
    {
        switch (dto.Event)
        {
            case AuthUserEvents.NewGameOffer:
                NewGameOffer?.Invoke(dto);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
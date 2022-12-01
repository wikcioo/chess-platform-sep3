using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Signalr;

public class HubConnectionDto
{
    public HubConnection? HubConnection { get; set; }
}
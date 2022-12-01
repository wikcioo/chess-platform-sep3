using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Signalr;

public class HubConnectionWrapper
{
    public HubConnection? HubConnection { get; set; }
}
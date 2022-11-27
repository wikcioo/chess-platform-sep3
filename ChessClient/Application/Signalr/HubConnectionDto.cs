using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Signalr;

public class HubConnectionDto
{
    public HubConnection? HubConnection { get; set; }
}
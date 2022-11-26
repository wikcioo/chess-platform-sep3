using Application.LogicInterfaces;
using Application.Signalr;
using Domain.DTOs.Chat;
using HttpClients.ClientInterfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.LogicImplementations;

public class ChatLogic : IChatLogic
{
    private readonly IAuthService _authService;

    public event Action<string> MessageReceived;
    private string _chatLog = "";

    public bool IsConnected =>
        _hubConnection?.State == HubConnectionState.Connected;

    private HubConnection? _hubConnection;
    private ulong _gameRoom;
    private readonly HubConnectionDto _hubDto;

    public ChatLogic(IAuthService authService, HubConnectionDto hubDto)
    {
        _authService = authService;
        _hubDto = hubDto;
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.SendAsync("SendMessage", dto.GameRoom, dto.Body);
        }
    }

    public async Task StartMessagingAsync(ulong gameRoom)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7289/gamehub",
                options => { options.AccessTokenProvider = () => Task.FromResult(_authService.GetJwtToken())!; })
            .Build();
        _hubDto._hubConnection = _hubConnection;
        _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            _chatLog += $"<div>{user}:{message}\n</div>";
            MessageReceived.Invoke(_chatLog);
        });
        _hubConnection.On<List<MessageDto>>("GetLog", (log) =>
        {
            foreach (var dto in log)
            {
                _chatLog += $"<div>{dto.Username}:{dto.Body}\n</div>";
            }

            MessageReceived.Invoke(_chatLog);
        });

        await _hubConnection.StartAsync();
        await _hubConnection.SendAsync("JoinRoom", gameRoom);
        _gameRoom = gameRoom;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            _chatLog = "";
            await _hubConnection.SendAsync("LeaveRoom", _gameRoom);
            await _hubConnection.DisposeAsync();
        }
    }
}
using Application.LogicInterfaces;
using Application.Signalr;
using Domain.DTOs;
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
        _hubDto.HubConnection?.State == HubConnectionState.Connected;

    private ulong _gameRoom;
    private readonly HubConnectionDto _hubDto;

    public ChatLogic(IAuthService authService, HubConnectionDto hubDto)
    {
        _authService = authService;
        _hubDto = hubDto;
        if (_hubDto.HubConnection is not null)
        {
            _hubDto.HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                _chatLog += $"<div>{user}:{message}\n</div>";
                MessageReceived.Invoke(_chatLog);
            });
            _hubDto.HubConnection.On<List<MessageDto>>("GetLog", (log) =>
            {
                foreach (var dto in log)
                {
                    _chatLog += $"<div>{dto.Username}:{dto.Body}\n</div>";
                }

                MessageReceived.Invoke(_chatLog);
            });
        }
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.SendAsync("SendMessage", dto.GameRoom, dto.Body);
        }
    }

    public void StartMessaging(ulong gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubDto.HubConnection is not null)
        {
            _chatLog = "";
            await _hubDto.HubConnection.SendAsync("LeaveRoom", _gameRoom);
        }
    }
}
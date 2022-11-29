using Application.LogicInterfaces;
using Application.Signalr;
using Domain.DTOs.Chat;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.LogicImplementations;

public class ChatLogic : IChatLogic
{
    public event Action<string>? MessageReceived;
    private string _chatLog = "";

    private readonly HubConnectionDto _hubDto;

    public ChatLogic(HubConnectionDto hubDto)
    {
        _hubDto = hubDto;
        if (_hubDto.HubConnection is null) return;

        _hubDto.HubConnection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            _chatLog += $"<div>{user}:{message}\n</div>";
            MessageReceived?.Invoke(_chatLog);
        });
        _hubDto.HubConnection.On<List<MessageDto>>("GetLog", (log) =>
        {
            foreach (var dto in log)
            {
                _chatLog += $"<div>{dto.Username}:{dto.Body}\n</div>";
            }

            MessageReceived?.Invoke(_chatLog);
        });
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.SendAsync("SendMessage", dto.GameRoom, dto.Body);
        }
    }

}
using System.Security.Claims;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.Chat;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using HttpClients.ClientInterfaces;

namespace Application.LogicImplementations;

public class ChatLogic : IChatLogic
{
    private Chat.ChatClient _client;
    private readonly IAuthService _authService;

    public event Action<string> MessageReceived;
    private AsyncServerStreamingCall<Message> _call;
    private string _chatLog = "";

    public ChatLogic(GrpcChannel channel, IAuthService authService)
    {
        _authService = authService;
        _client = new Chat.ChatClient(channel);
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        ClaimsPrincipal user = await _authService.GetAuthAsync();
        if (user.Identity == null)
            throw new InvalidOperationException("User not logged in.");
        await _client.WriteAsync(
            new Message
            {
                Username = user.Identity.Name!,
                Body = dto.Body,
                GameRoom = dto.GameRoom
            });
    }

    public async Task StartMessagingAsync(ulong gameRoom)
    {
        var user = await _authService.GetAuthAsync();
        if (user.Identity == null)
            throw new InvalidOperationException("User not logged in.");
        if (_call != null)
        {
            _call.Dispose();
            _chatLog = "";
        }

        _call = _client.StartMessaging(new RequestMessage
        {
            Username = user.Identity.Name,
            GameRoom = gameRoom
        });
        while (await _call.ResponseStream.MoveNext())
        {
            var message = _call.ResponseStream.Current;
            _chatLog += $"<div>{message.Username}:{message.Body}\n</div>";
            MessageReceived.Invoke(_chatLog);
        }
    }
}
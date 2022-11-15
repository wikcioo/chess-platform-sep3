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

    public event Action<MessageDto> MessageReceived;

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
                Receiver = dto.Receiver
            });
    }

    public async Task StartMessagingAsync(string opponentUsername)
    {
        ClaimsPrincipal user = await _authService.GetAuthAsync();

        if (user.Identity == null)
            throw new InvalidOperationException("User not logged in.");

        AsyncServerStreamingCall<Message> call = _client.StartMessaging(new RequestMessage
        {
            Username = user.Identity.Name!,
            Receiver = opponentUsername
        });
        while (await call.ResponseStream.MoveNext())
        {
            var message = call.ResponseStream.Current;
            MessageReceived.Invoke(new MessageDto
            {
                Username = message.Username,
                Body = message.Body,
                Receiver = message.Receiver
            });
        }
    }
}
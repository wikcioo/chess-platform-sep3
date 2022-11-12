using Application.LogicInterfaces;
using Domain.DTOs.Chat;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;

namespace Application.LogicImplementations;

public class ChatLogic : IChatLogic
{
    private Chat.ChatClient _client;

    public event Action<MessageDto> MessageReceived;

    public ChatLogic(GrpcChannel channel)
    {
        _client = new Chat.ChatClient(channel);
    }

    public async Task WriteMessageAsync(MessageDto dto)
    {
        await _client.WriteAsync(
            new Message {Username = dto.Username, Body = dto.Body, Receiver = dto.Receiver});
    }

    public async Task StartMessagingAsync(RequestMessageDto dto)
    {
        AsyncServerStreamingCall<Message> call = _client.StartMessaging(new RequestMessage
        {
            Username = dto.Username,
            Receiver = dto.Receiver
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
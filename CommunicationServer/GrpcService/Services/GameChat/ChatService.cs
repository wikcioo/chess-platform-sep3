using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.Chat;
using Grpc.Core;

namespace GrpcService.Services.GameChat;

public class ChatService : Chat.ChatBase
{
    private readonly IChatLogic _chatLogic;
    private readonly Empty _empty = new();

    public ChatService(IChatLogic chatRoomService)
    {
        _chatLogic = chatRoomService;
    }

    public override async Task StartMessaging(RequestMessage request, IServerStreamWriter<Message> responseStream,
        ServerCallContext context)
    {
        //Add listen here
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                await _chatLogic.GetMessagesAsObservable(new RequestMessageDto
                    {
                        GameRoom = request.GameRoom,
                    })
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async (x) => await responseStream.WriteAsync(new Message
                    {
                        Username = x.Username,
                        Body = x.Body,
                        GameRoom = x.GameRoom
                    }), context.CancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }


    public override Task<Empty> Write(Message request, ServerCallContext context)
    {
        //Invoke with the new message here
        _chatLogic.Add(new MessageDto
        {
            Username = request.Username,
            Body = request.Body,
            GameRoom = request.GameRoom
        });
        return Task.FromResult(_empty);
    }
}
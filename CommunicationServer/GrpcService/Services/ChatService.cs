using Grpc.Core;

namespace GrpcService.Services;

public class ChatService : Chat.ChatBase
{
    private ChatRoomService _service;
    private readonly Empty _empty = new();

    public ChatService(ChatRoomService chatRoomService)
    {
        _service = chatRoomService;
    }

    public override async Task StartGame(RequestGame request, IServerStreamWriter<Message> responseStream, ServerCallContext context)
    {
        
        //Add listen here
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                await _service.GetMessagesAsObservable(request)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async (x) => await responseStream.WriteAsync(x), context.CancellationToken)
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
        Console.WriteLine($"{request.Username}: {request.Body}");
        _service.Add(request);
        return Task.FromResult(_empty);
    }
}
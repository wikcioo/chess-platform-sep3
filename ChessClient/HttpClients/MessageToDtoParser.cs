using Domain.DTOs;
using GrpcService;

namespace HttpClients;

public static class MessageToDtoParser
{
    public static JoinedGameStreamDto ToDto(JoinedGameStream message)
    {
        return new JoinedGameStreamDto
        {
            FenString = message.Fen,
            GameEndType = message.GameEndType,
            TimeLeftMs = message.TimeLeftMs,
            IsWhite = message.IsWhite,
            Event = message.Event,
        };
    }
    
    public static ResponseGameDto ToDto(ResponseGame message)
    {
        return new ResponseGameDto
        {
            Success = message.Success,
            GameRoom = message.GameRoom,
            Opponent = message.Opponent,
            Fen = message.Opponent,
            IsWhite = message.IsWhite
        };
    }
}
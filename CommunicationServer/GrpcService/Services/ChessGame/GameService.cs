using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Enums;
using Grpc.Core;

namespace GrpcService.Services.ChessGame;

public class GameService : Game.GameBase
{
    private readonly IGameLogic _gameLogic;

    public GameService(IGameLogic gameLogic)
    {
        _gameLogic = gameLogic;
    }

    public override async Task<ResponseGame> StartGame(RequestGame request, ServerCallContext context)
    {
        RequestGameDto dto = new()
        {
            Username = request.Username,
            GameType = request.GameType,
            Increment = request.Increment,
            IsWhite = request.IsWhite,
            Opponent = request.Opponent,
            Seconds = request.Seconds
        };
        var responseDto = await _gameLogic.StartGame(dto);
        ResponseGame response = new()
        {
            Success = responseDto.Success,
            GameRoom = responseDto.GameRoom,
            Opponent = responseDto.Opponent,
            Fen = responseDto.Fen,
            IsWhite = responseDto.IsWhite,
        };
        return response;
    }

    public override async Task JoinGame(RequestJoinGame request, IServerStreamWriter<JoinedGameStream> responseStream,
        ServerCallContext context)
    {
        //Add listen here
        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                RequestJoinGameDto dto = new()
                {
                    GameRoom = request.GameRoom,
                    Username = request.Username
                };
                await _gameLogic.JoinGame(dto)
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(async (x) => await responseStream.WriteAsync(new JoinedGameStream()
                    {
                        Fen = x.FenString,
                        GameEndType = x.GameEndType,
                        TimeLeftMs = x.TimeLeftMs,
                        IsWhite = x.IsWhite
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

    public override async Task<Acknowledge> MakeMove(RequestMakeMove request, ServerCallContext context)
    {
        AckTypes ack = await _gameLogic.MakeMove(new MakeMoveDto()
        {
            GameRoom = request.GameRoom,
            FromSquare = request.FromSquare,
            ToSquare = request.ToSquare,
            MoveType = request.MoveType,
            Promotion = request.Promotion
        });
        return new Acknowledge()
        {
            Status = (uint) ack
        };
    }
}
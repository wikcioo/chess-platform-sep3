using System.Security.Claims;
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
        Enum.TryParse(request.Side, out GameSides side);
        Enum.TryParse(request.OpponentType, out OpponentTypes opponentType);
        
        RequestGameDto dto = new()
        {
            Username = request.Username,
            OpponentType = opponentType,
            OpponentName = request.OpponentName,
            Seconds = request.Seconds,
            Increment = request.Increment,
            Side = side,
            IsVisible = request.IsVisible
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
                        IsWhite = x.IsWhite,
                        Event = (uint) x.Event,
                        UsernameWhite = x.UsernameWhite,
                        UsernameBlack = x.UsernameBlack
                    }), context.CancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }

    public override async Task<Acknowledge> MakeMove(RequestMakeMove request, ServerCallContext context)
    {
        var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        if (claim == null)
        {
            return new Acknowledge()
            {
                Status = (uint) AckTypes.NotUserTurn
            };
        }

        AckTypes ack = await _gameLogic.MakeMove(new MakeMoveDto()
        {
            GameRoom = request.GameRoom,
            FromSquare = request.FromSquare,
            ToSquare = request.ToSquare,
            MoveType = request.MoveType,
            Promotion = request.Promotion,
            Username = claim.Value
        });
        return new Acknowledge()
        {
            Status = (uint) ack
        };
    }

    public override async Task<Acknowledge> Resign(RequestResign request, ServerCallContext context)
    {
        var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        if (claim == null)
        {
            return new Acknowledge()
            {
                Status = (uint) AckTypes.NotUserTurn
            };
        }

        AckTypes ack = await _gameLogic.Resign(new RequestResignDto()
        {
            GameRoom = request.GameRoom,
            Username = claim.Value
        });
        return new Acknowledge()
        {
            Status = (uint)ack
        };
    }

    public override async Task<Acknowledge> OfferDraw(RequestDraw request, ServerCallContext context)
    {
        var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        if (claim == null)
        {
            return new Acknowledge()
            {
                Status = (uint) AckTypes.NotUserTurn
            };
        }
        
        AckTypes ack = await _gameLogic.OfferDraw(new RequestDrawDto()
        {
            GameRoom = request.GameRoom,
            Username = claim.Value
        });
        return new Acknowledge()
        {
            Status = (uint)ack
        };
    }

    public override async Task<Acknowledge> DrawOfferResponse(ResponseDraw request, ServerCallContext context)
    {
        var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        if (claim == null)
        {
            return new Acknowledge()
            {
                Status = (uint) AckTypes.NotUserTurn
            };
        }
        
        AckTypes ack = await _gameLogic.DrawOfferResponse(new ResponseDrawDto()
        {
            GameRoom = request.GameRoom,
            Username = claim.Value,
            Accept = request.Accept
        });
        return new Acknowledge()
        {
            Status = (uint)ack
        };
    }

    public override Task<ResponseSpectateableGameRooms> GetSpectateableGames(EmptyGameMessage request, ServerCallContext context)
    {
        var responseSpectateableGameRooms = new ResponseSpectateableGameRooms();
        foreach (var room in _gameLogic.GetSpectateableGameRoomData())
        {
            responseSpectateableGameRooms.GameRoomsData.Add(new SpectateableGameRoomData()
            {
                GameRoom = room.GameRoom,
                UsernameWhite = room.UsernameWhite,
                UsernameBlack = room.UsernameBlack,
                Seconds = room.Seconds,
                Increment = room.Increment
            });
        }

        return Task.FromResult(responseSpectateableGameRooms);
    }

    public override Task<ResponseJoinableGameRooms> GetJoinableGames(EmptyGameMessage request, ServerCallContext context)
    {
        var responseJoinableGameRooms = new ResponseJoinableGameRooms();
        foreach (var room in _gameLogic.GetJoinableGameRoomData())
        {
            responseJoinableGameRooms.GameRoomsData.Add(new JoinableGameRoomData()
            {
                GameRoom = room.GameRoom,
                Username = room.Username,
                Seconds = room.Seconds,
                Increment = room.Increment
            });
        }

        return Task.FromResult(responseJoinableGameRooms);
    }
}
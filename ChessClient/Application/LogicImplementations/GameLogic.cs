using System.Security.Claims;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Enums;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using HttpClients;
using HttpClients.ClientInterfaces;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Types;

namespace Application.LogicImplementations;

public class GameLogic : IGameLogic
{
    private readonly IAuthService _authService;
    private readonly Game.GameClient _gameClient;
    
    public bool IsDrawOfferPending { get; set; } = false;
    public bool OnWhiteSide { get; set; } = true;
    public bool WhiteTurn { get; private set; } = true;
    public ulong? GameRoomId { get; set; }


    //Todo Possibility of replacing StreamUpdate with action and only needed information instead of dto
    public delegate void StreamUpdate(JoinedGameStreamDto dto);
    public event StreamUpdate? TimeUpdated;
    public event StreamUpdate? NewFenReceived;
    public event StreamUpdate? ResignationReceived;
    public event StreamUpdate? InitialTimeReceived;
    public event StreamUpdate? DrawOffered;
    public event StreamUpdate? DrawOfferTimedOut;
    public event StreamUpdate? DrawOfferAccepted;
    public GameLogic(IAuthService authService, GrpcChannel channel)
    {
        _authService = authService;
        _gameClient = new Game.GameClient(channel);
    }


    public async Task<ResponseGameDto> CreateGame(RequestGameDto dto)
    {
        ClaimsPrincipal user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");
        
        try
        {
            var grpcResponse = await _gameClient.StartGameAsync(new RequestGame()
            {
                GameType = dto.GameType,
                Increment = dto.Increment,
                //TODO Add a randomizer for side when not chosen
                IsWhite = dto.IsWhite ?? true,
                Opponent = dto.Opponent,
                Seconds = dto.Seconds,
                Username = user.Identity?.Name
            });

            ResponseGameDto response = MessageToDtoParser.ToDto(grpcResponse);

            OnWhiteSide = response.IsWhite;
            GameRoomId = response.GameRoom;
            
            return response;
        }
        catch (RpcException e)
        {
            throw new HttpRequestException("Failed to connect to server", e);
        }
    }

    public async Task JoinGame(RequestJoinGameDto dto)
    {
        AsyncServerStreamingCall<JoinedGameStream>? call;
        
        try
        {
            call = _gameClient.JoinGame(new RequestJoinGame()
            {
                GameRoom = dto.GameRoom,
                Username = "Bob"
            });
        }
        catch (ArgumentException)
        {
            //TODO: This catch will never catch anything cause there is no error thrown. New grpc message is needed
            throw new ArgumentException("Game room not found");
        }

        try
        {
            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                var response = MessageToDtoParser.ToDto(call.ResponseStream.Current);

                switch (response.Event)
                {
                    case GameStreamEvents.NewFenPosition:
                        NewFenReceived?.Invoke(response);
                        break;
                    case GameStreamEvents.TimeUpdate:
                        TimeUpdate(response);
                        break;
                    case GameStreamEvents.Resignation:
                        ResignationReceived?.Invoke(response);
                        break;
                    case GameStreamEvents.DrawOffer:
                        DrawOffer(response);
                        break;
                    case GameStreamEvents.DrawOfferTimeout:
                        DrawOfferTimeout(response);
                        break;
                    case GameStreamEvents.DrawOfferAcceptation:
                        DrawOfferAcceptation(response);
                        break;
                    case GameStreamEvents.InitialTime:
                        InitialTimeReceived?.Invoke(response);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch (RpcException)
        {
            throw new HttpRequestException("Connection error. Failed to participate in game stream");
        }
        
    }

    private void TimeUpdate(JoinedGameStreamDto dto)
    {
        if (dto.GameEndType != (uint)GameEndTypes.TimeIsUp)
        {
            TimeUpdated?.Invoke(dto);
        }
    }
    private void DrawOffer(JoinedGameStreamDto dto)
    {
        if (!(dto.IsWhite && OnWhiteSide) && !(!dto.IsWhite && !OnWhiteSide))
            IsDrawOfferPending = true;
        DrawOffered?.Invoke(dto);
    }
    
    private void DrawOfferTimeout(JoinedGameStreamDto dto)
    {
        IsDrawOfferPending = false;
        DrawOfferTimedOut?.Invoke(dto);
    }
    
    private void DrawOfferAcceptation(JoinedGameStreamDto dto)
    {
        IsDrawOfferPending = false;
        DrawOfferAccepted?.Invoke(dto);
    }

    public async Task<int> MakeMove(Move move)
    {
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");
        
        
        var token = _authService.GetJwtToken();
        var headers = new Metadata { { "Authorization", $"Bearer {token}" } };
        var call = await _gameClient.MakeMoveAsync(new RequestMakeMove
        {
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            GameRoom = GameRoomId.Value,
            MoveType = (uint)move.MoveType(),
            Promotion = (uint)move.PromotedPieceType().AsInt()
        }, headers);
        return (int)call.Status;
    }

    public async Task<AckTypes> OfferDraw()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;
        
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        
        if (!isLoggedIn) 
            throw new InvalidOperationException("User not logged in");


        var token = _authService.GetJwtToken();
        var headers = new Metadata { { "Authorization", $"Bearer {token}" } };
        var ack = await _gameClient.OfferDrawAsync(new RequestDraw
        {
            Username = user.Identity!.Name, GameRoom = GameRoomId.Value
        }, headers);

        return (AckTypes) ack.Status;
    }

    public async Task<AckTypes> Resign()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        
        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");

        var token = _authService.GetJwtToken();
        var headers = new Metadata { { "Authorization", $"Bearer {token}" } };
        var ack =  await _gameClient.ResignAsync(new RequestResign { Username = user.Identity!.Name, GameRoom = GameRoomId.Value }, headers);
         return (AckTypes) ack.Status;
    }

    public async Task<AckTypes> SendDrawResponse(bool accepted)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn) 
            throw new InvalidOperationException("User not logged in.");

        var token = _authService.GetJwtToken();
        var headers = new Metadata { { "Authorization", $"Bearer {token}" } };
        var ack = await _gameClient.DrawOfferResponseAsync(new ResponseDraw
        {
            Username = user.Identity!.Name, GameRoom = GameRoomId.Value, Accept = accepted
        }, headers);

        return (AckTypes) ack.Status;
    }
}
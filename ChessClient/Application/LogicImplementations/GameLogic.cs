using System.Security.Claims;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
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
    private EmptyGameMessage _empty = new();
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
    public event StreamUpdate? EndOfTheGameReached;
    public event StreamUpdate? GameFirstJoined;

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
                OpponentType = dto.OpponentType.ToString(),
                Increment = dto.Increment,
                Side = dto.Side.ToString(),
                OpponentName = dto.OpponentName,
                Seconds = dto.Seconds,
                Username = user.Identity?.Name,
                IsVisible = dto.IsVisible
            });

            ResponseGameDto response = MessageToDtoParser.ToDto(grpcResponse);

            OnWhiteSide = response.IsWhite;

            return response;
        }
        catch (RpcException e)
        {
            throw new HttpRequestException("Failed to connect to server", e);
        }
    }

    public async Task JoinGame(RequestJoinGameDto dto)
    {
        ClaimsPrincipal user = await _authService.GetAuthAsync();

        Console.WriteLine(user.Identity?.Name);

        AsyncServerStreamingCall<JoinedGameStream>? call;

        try
        {
            call = _gameClient.JoinGame(new RequestJoinGame()
            {
                GameRoom = dto.GameRoom,
                Username = user.Identity?.Name
            });

            GameRoomId = dto.GameRoom;
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
                        TimeUpdate(response);
                        break;
                    case GameStreamEvents.TimeUpdate:
                        if (response.GameEndType == (uint) GameEndTypes.TimeIsUp) call.Dispose();
                        TimeUpdate(response);
                        break;
                    case GameStreamEvents.ReachedEndOfTheGame:
                        call.Dispose();
                        TimeUpdate(response);
                        NewFenReceived?.Invoke(response);
                        EndOfTheGameReached?.Invoke(response);
                        break;
                    case GameStreamEvents.Resignation:
                        call.Dispose();
                        ResignationReceived?.Invoke(response);
                        break;
                    case GameStreamEvents.DrawOffer:
                        DrawOffer(response);
                        break;
                    case GameStreamEvents.DrawOfferTimeout:
                        DrawOfferTimeout(response);
                        break;
                    case GameStreamEvents.DrawOfferAcceptation:
                        call.Dispose();
                        DrawOfferAcceptation(response);
                        break;
                    case GameStreamEvents.InitialTime:
                        InitialTime(response);
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
        TimeUpdated?.Invoke(dto);
    }

    private async void InitialTime(JoinedGameStreamDto dto)
    {
        var user = await _authService.GetAuthAsync();
        var myName = user.Identity!.Name;
        if (dto.UsernameBlack.Equals(myName))
        {
            OnWhiteSide = false;
        }

        if (dto.UsernameWhite.Equals(myName))
        {
            OnWhiteSide = true;
        }

        GameFirstJoined?.Invoke(dto);
        InitialTimeReceived?.Invoke(dto);
    }

    private async void DrawOffer(JoinedGameStreamDto dto)
    {
        var user = await _authService.GetAuthAsync();
        if (dto.UsernameWhite.Equals(user.Identity!.Name) || dto.UsernameBlack.Equals(user.Identity!.Name))
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
        var headers = new Metadata {{"Authorization", $"Bearer {token}"}};
        var call = await _gameClient.MakeMoveAsync(new RequestMakeMove
        {
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            GameRoom = GameRoomId.Value,
            MoveType = (uint) move.MoveType(),
            Promotion = (uint) move.PromotedPieceType().AsInt()
        }, headers);
        return (int) call.Status;
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
        var headers = new Metadata {{"Authorization", $"Bearer {token}"}};
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
        var headers = new Metadata {{"Authorization", $"Bearer {token}"}};
        var ack = await _gameClient.ResignAsync(
            new RequestResign {Username = user.Identity!.Name, GameRoom = GameRoomId.Value}, headers);
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
        var headers = new Metadata {{"Authorization", $"Bearer {token}"}};
        var ack = await _gameClient.DrawOfferResponseAsync(new ResponseDraw
        {
            Username = user.Identity!.Name, GameRoom = GameRoomId.Value, Accept = accepted
        }, headers);

        if ((AckTypes) ack.Status == AckTypes.Success)
        {
            IsDrawOfferPending = false;
        }

        return (AckTypes) ack.Status;
    }

    public async Task<IList<SpectateableGameRoomDataDto>> GetAllSpectateableGames()
    {
        var response = await _gameClient.GetSpectateableGamesAsync(_empty);
        var roomList = response.GameRoomsData;

        return roomList.Select(room => new SpectateableGameRoomDataDto
            {
                GameRoom = room.GameRoom,
                Increment = room.Increment,
                Seconds = room.Seconds,
                UsernameWhite = room.UsernameWhite,
                UsernameBlack = room.UsernameBlack
            })
            .ToList();
    }

    public async Task<IList<JoinableGameRoomDataDto>> GetAllJoinableGames()
    {
        var response = await _gameClient.GetJoinableGamesAsync(_empty);
        var roomList = response.GameRoomsData;
        IList<JoinableGameRoomDataDto> joinableRoomList = new List<JoinableGameRoomDataDto>();
        foreach (var room in roomList)
        {
            Enum.TryParse(room.Side, out GameSides side);
            joinableRoomList.Add(new JoinableGameRoomDataDto
            {
                GameRoom = room.GameRoom,
                Increment = room.Increment,
                Seconds = room.Seconds,
                Side = side,
                Username = room.Username
            });
        }

        return joinableRoomList;
    }
}
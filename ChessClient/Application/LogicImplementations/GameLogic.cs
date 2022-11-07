using System.Security.Claims;
using Application.LogicInterfaces;
using Domain.DTOs;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;
using HttpClients;
using HttpClients.ClientInterfaces;

namespace Application.LogicImplementations;

public class GameLogic : IGameLogic
{
    private readonly IAuthService _authService;
    private readonly Game.GameClient _gameClient;

    private bool _isDrawOfferPending = false;
    
    public bool OnWhiteSide { get; set; }
    
    public delegate void NextMove(JoinedGameStreamDto dto);

    public event NextMove? GameStreamReceived;

    public GameLogic(IAuthService authService, GrpcChannel channel)
    {
        _authService = authService;
        _gameClient = new Game.GameClient(channel);
    }


    public async Task<ResponseGameDto> CreateGame(RequestGameDto dto)
    {
        ClaimsPrincipal user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn is { IsAuthenticated: false })
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
                //TODO remove null suppression, add actual checks
                Username = user.Identity!.Name!
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
                var message = call.ResponseStream.Current;
                GameStreamReceived?.Invoke(MessageToDtoParser.ToDto(message));
            }
        }
        catch (RpcException)
        {
            throw new HttpRequestException("Connection error. Failed to participate in game stream");
        }
        
    }
}
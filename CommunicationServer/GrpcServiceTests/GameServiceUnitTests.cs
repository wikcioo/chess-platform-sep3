using System.Globalization;
using System.Security.Claims;
using Application.Logic;
using DatabaseClient.Implementations;
using Domain.Enums;
using GrpcService;
using GrpcService.Services.ChessGame;
using GrpcServiceTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GrpcServiceTests;

public class GameServiceUnitTests
{
    private readonly RequestGame _requestGame;
    private readonly GameService _gameService;


    public GameServiceUnitTests()
    {
        _requestGame = new RequestGame
        {
            Username = "Jeff",
            GameType = "Friend",
            Increment = 0,
            IsWhite = true,
            Opponent = "Alice",
            Seconds = 600
        };
        _gameService = new GameService(new GameLogic(new StockfishHttpClient(new HttpClient())));
    }

    //Starting a game
    [Fact]
    public async Task StartingGameHasCorrectOpponent()
    {
        var response = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());

        Assert.Equal(_requestGame.Opponent, response.Opponent);
    }

    //Joining a game
    // [Fact]
    // public async Task JoiningGame()
    // {
    //     var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
    //     var cts = new CancellationTokenSource();
    //     var callContext = TestServerCallContext.Create(cancellationToken: cts.Token);
    //     var responseStream = new TestServerStreamWriter<JoinedGameStream>(callContext);
    //     var response = _gameService.JoinGame(new RequestJoinGame
    //     {
    //         GameRoom = responseGame.GameRoom,
    //         Username = "Jeff"
    //     }, responseStream, callContext);
    //     await response;
    //     var reader = new TestAsyncStreamReader<JoinedGameStream>(callContext);
    //     while (await reader.MoveNext())
    //     {
    //         var resp = reader.Current;
    //         _testOutputHelper.WriteLine(resp.Fen);
    //     }
    // }

    //Making a move
    [Fact]
    public async Task MakingAValidMoveAsValidUserReturnsSuccess()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpAuthenticated("Jeff");
        var response = await _gameService.MakeMove(new RequestMakeMove
        {
            FromSquare = "e2",
            ToSquare = "e4",
            Username = "Jeff",
            GameRoom = responseGame.GameRoom,
            MoveType = 0,
            Promotion = 0
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.Success, response.Status);
    }

    [Fact]
    public async Task MakingAnInvalidMoveAsValidUserReturnsInvalidMove()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpAuthenticated("Jeff");
        var response = await _gameService.MakeMove(new RequestMakeMove
        {
            FromSquare = "e2",
            ToSquare = "e2",
            Username = "Jeff",
            GameRoom = responseGame.GameRoom,
            MoveType = 0,
            Promotion = 0
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.InvalidMove, response.Status);
    }

    [Fact]
    public async Task MakingAValidMoveAsInvalidUserReturnsNotUserTurn()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpNotAuthenticated();
        var response = await _gameService.MakeMove(new RequestMakeMove
        {
            FromSquare = "e2",
            ToSquare = "e2",
            Username = "Bob",
            GameRoom = responseGame.GameRoom,
            MoveType = 0,
            Promotion = 0
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.NotUserTurn, response.Status);
    }

    //Offering a draw
    [Fact]
    public async Task OfferingADrawWithInvalidUserReturnsNotUserTurn()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpNotAuthenticated();
        var response = await _gameService.OfferDraw(new RequestDraw
        {
            GameRoom = responseGame.GameRoom,
            Username = "Jeff"
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.NotUserTurn, response.Status);
    }

    //Resigning
    [Fact]
    public async Task ResigningWithValidUserReturnsSuccess()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpAuthenticated("Jeff");
        var response = await _gameService.Resign(new RequestResign
        {
            GameRoom = responseGame.GameRoom,
            Username = "Jeff"
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.Success, response.Status);
    }

    [Fact]
    public async Task ResigningWithInValidUserReturnsNotUserTurn()
    {
        var responseGame = await _gameService.StartGame(_requestGame, TestServerCallContext.Create());
        var serverCallContext = SetUpNotAuthenticated();
        var response = await _gameService.Resign(new RequestResign
        {
            GameRoom = responseGame.GameRoom,
            Username = "Bob"
        }, serverCallContext);
        Assert.Equal((uint) AckTypes.NotUserTurn, response.Status);
    }

    //Helper methods
    private static TestServerCallContext SetUpAuthenticated(string name)
    {
        var httpContext = new DefaultHttpContext();
        ClaimsIdentity identity = new(CreateClaims(name), "jwt");
        httpContext.User.AddIdentity(identity);
        var serverCallContext = TestServerCallContext.Create();
        serverCallContext.UserState["__HttpContext"] = httpContext;
        return serverCallContext;
    }

    private static TestServerCallContext SetUpNotAuthenticated()
    {
        var httpContext = new DefaultHttpContext();
        var serverCallContext = TestServerCallContext.Create();
        serverCallContext.UserState["__HttpContext"] = httpContext;
        return serverCallContext;
    }

    private static IEnumerable<Claim> CreateClaims(string name)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, "user"),
            new Claim("Email", "Jeff@gmail.com")
        };
        return claims;
    }
}
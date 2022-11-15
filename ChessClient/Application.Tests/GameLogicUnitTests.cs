using System.Security.Claims;
using Application.LogicImplementations;
using Application.Tests.Helpers;
using Domain.DTOs;
using Domain.Enums;
using Grpc.Core;
using GrpcService;
using HttpClients.ClientInterfaces;
using Moq;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;
using Xunit.Abstractions;

namespace Application.Tests;

public class GameLogicUnitTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GameLogicUnitTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static GameLogic CreateEmptyMockGameLogic()
    {
        var mockAuth = new Mock<IAuthService>();
        var mockClient = new Mock<Game.GameClient>();
        return new GameLogic(mockAuth.Object, mockClient.Object);
    }
    
    [Fact]
    public void CreateGameThrowsInvalidOperationExceptionWhenNoValidClaims()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.CreateGame(new RequestGameDto()));
    }

    [Fact]
    public void CreateGameThrowsRpcExceptionWhenNoValidGrpcClient()
    {
        var mockClient = new Mock<Game.GameClient>();
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Bob"),

            }, "mock"))));
        
        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object);
        Assert.ThrowsAsync<RpcException>(() => gameLogic.CreateGame(new RequestGameDto()));
    }

    [Fact]
    public void CreateGameIsSuccessfulWhenCorrectClaimAndValidGrpcClient()
    {
        var request = new RequestGameDto()
        {
            Username = "Bob",
            OpponentType = OpponentTypes.Friend,
            OpponentName = "Jim",
            Side = GameSides.White,
            Seconds = 60,
            Increment = 0,
            IsVisible = true
        };

        var response = new ResponseGameDto()
        {
            Success = true,
            GameRoom = 0,
            Fen = Fen.StartPositionFen,
            Opponent = "Jim",
            IsWhite = true
        };
        
        var mockCall = CallHelpers.CreateAsyncUnaryCall(new ResponseGame()
        {
            Success = response.Success,
            GameRoom = response.GameRoom,
            Fen = response.Fen,
            Opponent = response.Opponent,
            IsWhite = response.IsWhite
        });
        
        var mockClient = new Mock<Game.GameClient>();
        mockClient
            .Setup(m => m.StartGameAsync(new RequestGame()
            {
                Username = request.Username,
                OpponentType = request.OpponentType.ToString(),
                OpponentName = request.OpponentName,
                Side = request.Side.ToString(),
                Seconds = request.Seconds,
                Increment = request.Increment,
                IsVisible = request.IsVisible
            }, null, null, CancellationToken.None))
            .Returns(mockCall);

        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Bob"),
        
            }, "mock"))));

        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object);
        var res = gameLogic.CreateGame(request).Result;

        Assert.Equal(response.Fen, res.Fen);
        Assert.Equal(response.Success, res.Success);
        Assert.Equal(response.GameRoom, res.GameRoom);
        Assert.Equal(response.Opponent, res.Opponent);
        Assert.Equal(response.IsWhite, res.IsWhite);
    }
    
    [Fact]
    public void JoinGameThrowsInvalidOperationExceptionWhenNoValidClaims()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.JoinGame(new RequestJoinGameDto()));
    }

    [Fact]
    public void JoinGameThrowsArgumentExceptionWhenNoValidGrpcClient()
    {
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Bob"),
        
            }, "mock"))));
        
        var mockClient = new Mock<Game.GameClient>();

        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object);
        Assert.ThrowsAsync<ArgumentException>(() => gameLogic.JoinGame(new RequestJoinGameDto()));
    }

    [Fact]
    public void MakeMoveThrowsInvalidOperationExceptionWhenGameRoomIdNotSet()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.MakeMove(new Move()));
    }

    [Fact]
    public void MakeMoveReturnsSuccessAcknowledgeWhenJoinedGame()
    {
        var mockCall = CallHelpers.CreateAsyncUnaryCall(new Acknowledge()
        {
            Status = (uint)AckTypes.Success
        });

        const ulong gameRoomId = 0;
        var move = new Move(Square.A1, Square.A3, MoveTypes.Normal, PieceTypes.Queen);
        
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Jim"),
        
            }, "mock"))));
        mockAuth
            .Setup(service => service.GetJwtToken())
            .Returns("foo");

        var mockClient = new Mock<Game.GameClient>();
        mockClient
            .Setup(m => m.MakeMoveAsync(It.IsAny<RequestMakeMove>(), It.IsAny<Metadata>(), null,CancellationToken.None))
            .Returns(mockCall);
       
        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object)
        {
            GameRoomId = gameRoomId
        };

        var res = gameLogic.MakeMove(move).Result;
        Assert.Equal((int)AckTypes.Success, res);
        
    }

    [Fact]
    public void OfferDrawThrowsInvalidOperationExceptionWhenGameRoomIdNotSet()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.OfferDraw());
    }
    
    [Fact]
    public void OfferDrawThrowsInvalidOperationExceptionWhenNoValidClaims()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.OfferDraw());
    }

    [Fact]
    public void OfferDrawReturnsSuccessAcknowledgeWhenValid()
    {
        var mockCall = CallHelpers.CreateAsyncUnaryCall(new Acknowledge()
        {
            Status = (uint)AckTypes.Success
        });
        
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Bob"),
        
            }, "mock"))));
        
        var mockClient = new Mock<Game.GameClient>();
        mockClient
            .Setup(m => m.OfferDrawAsync(It.IsAny<RequestDraw>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(mockCall);

        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object)
        {
            GameRoomId = 0
        };

        var res = gameLogic.OfferDraw().Result;
        Assert.Equal(AckTypes.Success, res);
    }
    
    [Fact]
    public void ResignThrowsInvalidOperationExceptionWhenGameRoomIdNotSet()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.Resign());
    }
    
    [Fact]
    public void ResignThrowsInvalidOperationExceptionWhenNoValidClaims()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.Resign());
    }
    
    [Fact]
    public async Task ResignReturnsSuccessAcknowledgeWhenValid()
    {
        var mockCall = CallHelpers.CreateAsyncUnaryCall(new Acknowledge()
        {
            Status = (uint)AckTypes.Success
        });
        
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Bob"),
        
            }, "mock"))));
        
        var mockClient = new Mock<Game.GameClient>();
        mockClient
            .Setup(m => m.ResignAsync(It.IsAny<RequestResign>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(mockCall);

        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object)
        {
            GameRoomId = 0
        };

        var res = await gameLogic.Resign();
        _testOutputHelper.WriteLine(res.ToString());
        Assert.Equal(AckTypes.Success, res);
    }
    
    [Fact]
    public void SendDrawResponseThrowsInvalidOperationExceptionWhenGameRoomIdNotSet()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.SendDrawResponse(true));
    }
    
    [Fact]
    public void SendDrawResponseThrowsInvalidOperationExceptionWhenNoValidClaims()
    {
        var gameLogic = CreateEmptyMockGameLogic();
        Assert.ThrowsAsync<InvalidOperationException>(() => gameLogic.SendDrawResponse(true));
    }

    [Fact]
    public async void SendDrawResponseClearsIsDrawOfferPendingFlagWhenPassedAccept()
    {
        var mockCall = CallHelpers.CreateAsyncUnaryCall(new Acknowledge()
        {
            Status = (uint)AckTypes.Success
        });
        
        var mockAuth = new Mock<IAuthService>();
        mockAuth
            .Setup(service => service.GetAuthAsync())
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "Jim"),
        
            }, "mock"))));
        var mockClient = new Mock<Game.GameClient>();
        mockClient
            .Setup(m => m.DrawOfferResponseAsync(It.IsAny<ResponseDraw>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(mockCall);

        var gameLogic = new GameLogic(mockAuth.Object, mockClient.Object)
        {
            IsDrawOfferPending = true,
            GameRoomId = 0
        };

        await gameLogic.SendDrawResponse(true);
        Assert.False(gameLogic.IsDrawOfferPending);
    }
}
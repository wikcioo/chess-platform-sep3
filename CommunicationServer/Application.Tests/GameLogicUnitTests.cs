using System.Collections.Generic;
using Application.Logic;
using Application.LogicInterfaces;
using Domain.Enums;
using Rudzoft.ChessLib.Fen;

namespace Application.Tests;

using ClientInterfaces;
using Domain.DTOs.Draw;
using Domain.DTOs.Game;
using Domain.DTOs.Resignation;
using Domain.DTOs.StartGame;
using Domain.DTOs.Stockfish;
using Domain.Models;
using GameRoomHandlers;
using Moq;
using System;
using System.Threading.Tasks;


public class GameLogicUnitTests
{

    private readonly IGameLogic _gameLogic;
    private readonly Mock<IUserService> _userServiceMock;

    public GameLogicUnitTests()
    {
        var stockfishServiceMock = new Mock<IStockfishService>();
        stockfishServiceMock.Setup(p => p.GetBestMoveAsync(It.IsAny<StockfishBestMoveDto>())).ReturnsAsync("e2e4");
        _userServiceMock = new Mock<IUserService>();
        _userServiceMock.Setup(p => p.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(new User());
        var gameServiceMock = new Mock<IGameService>();
        _gameLogic = new GameLogic(stockfishServiceMock.Object, new ChatLogic(), _userServiceMock.Object, gameServiceMock.Object, new GameRoomHandlerFactory());
    }

    private async Task<ulong> StartGameAndMakeActive(bool isVisible)
    {
        var response = await _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            DurationSeconds = 60,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = "Alice",
            IsVisible = isVisible
        });
        var requestDto = new RequestJoinGameDto
        {
            GameRoom = response.GameRoom,
            Username = "Jeff"
        };
        _gameLogic.JoinGame(requestDto);
        requestDto.Username = "Alice";
        _gameLogic.JoinGame(requestDto);
        return response.GameRoom;
    }


    //Start game
    [Theory]
    [InlineData("Alice", OpponentTypes.Friend)]
    [InlineData("StockfishAi1", OpponentTypes.Ai)]
    public void StartingGameReturnsCorrectResponseDto(string? opponent, OpponentTypes opponentType)
    {
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = opponentType,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = opponent,
            DurationSeconds = 600
        });

        Assert.True(response.Result.Success);
        Assert.Equal(opponent, response.Result.Opponent);
        Assert.Equal(Fen.StartPositionFen, response.Result.Fen);
    }


    [Theory]
    [InlineData(0, 86_401)]
    [InlineData(61, 30)]
    public void StartingGameWithInvalidTimeControlFails(uint increment, uint duration)
    {
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            DurationSeconds = duration,
            IncrementSeconds = increment,
            Side = GameSides.Black,
            OpponentName = "Alice"
        });

        Assert.False(response.Result.Success);
    }
    [Theory]
    [InlineData(0, 86_400)]
    [InlineData(60, 30)]
    public void StartingGameWithValidTimeControlReturnsTrue(uint increment, uint duration)
    {
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            DurationSeconds = duration,
            IncrementSeconds = increment,
            Side = GameSides.Black,
            OpponentName = "Alice"
        });

        Assert.True(response.Result.Success);
    }

    [Theory]
    [InlineData("Alice", OpponentTypes.Friend)]
    [InlineData("Bob", OpponentTypes.Friend)]
    public void StartingGameReturnsFalseWhenUserNotFound(string? opponent, OpponentTypes opponentType)
    {
        _userServiceMock.Setup(p => p.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = opponentType,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = opponent,
            DurationSeconds = 600
        });

        Assert.False(response.Result.Success);
    }

    [Theory]
    [InlineData("Alice", OpponentTypes.Random)]
    [InlineData("StockfishAi1", OpponentTypes.Random)]
    public void StartingGameAgainstRandomWithOpponentSetFails(string opponent, OpponentTypes opponentType)
    {
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = opponentType,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = opponent,
            DurationSeconds = 600
        });

        Assert.False(response.Result.Success);
        Assert.Equal("Opponent cannot be chosen for a random game.", response.Result.ErrorMessage);
    }

    [Theory]
    [InlineData("StockfishAi1", OpponentTypes.Friend)]
    public void StartingGameAgainstFriendWithStockfishOpponentFails(string opponent, OpponentTypes opponentType)
    {
        var response = _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = opponentType,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = opponent,
            DurationSeconds = 600
        });
        Assert.False(response.Result.Success);
        Assert.Equal("Opponent is an AI in the not ai game.", response.Result.ErrorMessage);
    }


    //Make move
    [Fact]
    public void MakeMoveReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = _gameLogic.MakeMove(new MakeMoveDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }

    //Resign
    [Fact]
    public void ResignReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = _gameLogic.Resign(new RequestResignDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
    //Offer Draw
    [Fact]
    public void OfferDrawReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = _gameLogic.OfferDraw(new RequestDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
    //Draw offer response
    [Fact]
    public void DrawOfferResponseReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = _gameLogic.DrawOfferResponse(new ResponseDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }

    //Spectate game
    [Fact]
    public async void SpectatingGameWhenVisibleReturnsSuccess()
    {
        var gameRoomId = await StartGameAndMakeActive(true);
        var requestDto = new RequestJoinGameDto
        {
            GameRoom = gameRoomId,
            Username = "Bob"
        };
        var ackResponse = _gameLogic.SpectateGame(requestDto);

        Assert.Equal(AckTypes.Success, ackResponse);
    }

    [Fact]
    public async void SpectatingGameWhenNotVisibleThrowsArgumentException()
    {
        var gameRoomId = await StartGameAndMakeActive(false);
        var requestDto = new RequestJoinGameDto
        {
            GameRoom = gameRoomId,
            Username = "Bob"
        };

        Assert.Throws<ArgumentException>(() => _gameLogic.SpectateGame(requestDto));
    }
    //Join game

    [Fact]
    public async void JoiningGameWhenValidReturnsSuccess()
    {
        var response = await _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            DurationSeconds = 60,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = "Alice",
            IsVisible = false
        });
        var requestDto = new RequestJoinGameDto
        {
            GameRoom = response.GameRoom,
            Username = "Jeff"
        };
        var ackResponse = _gameLogic.JoinGame(requestDto);


        Assert.Equal(AckTypes.Success, ackResponse);
    }

    [Fact]
    public async void JoiningGameWhenInvalidThrowsArgumentException()
    {
        var response = await _gameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            DurationSeconds = 60,
            IncrementSeconds = 0,
            Side = GameSides.Black,
            OpponentName = "Alice",
            IsVisible = false
        });
        var requestDto = new RequestJoinGameDto
        {
            GameRoom = response.GameRoom,
            Username = "Bob"
        };


        Assert.Throws<ArgumentException>(() => _gameLogic.JoinGame(requestDto));
    }

    [Fact]
    public void JoinRoomThrowsArgumentExceptionWhenNoRoomFound()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            _gameLogic.JoinGame(new RequestJoinGameDto()
            {
                Username = "Jeff",
                GameRoom = 0
            });
        });
    }
    //Resign


}
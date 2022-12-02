using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Entities;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using Rudzoft.ChessLib.Types;
using Xunit;

namespace Application.Tests;

public class GameRoomUnitTests
{
    private GameRoom? _gameRoom;
    private const string PlayerWhite = "Bob";
    private const string PlayerBlack = "Jim";
    private const uint TimeControlSeconds = 60;
    private const uint TimeControlIncrement = 5;

    private GameRoom GetNewGameRoom()
    {
        var gameRoom = new GameRoom(TimeControlSeconds, TimeControlIncrement, false, OpponentTypes.Random)
        {
            PlayerWhite = PlayerWhite,
            PlayerBlack = PlayerBlack,
            GameIsActive = true
        };
        return gameRoom;
    }

    [Theory]
    [InlineData(PlayerWhite)]
    [InlineData(PlayerBlack)]
    [InlineData("ElonMusk")]
    public void ResignReturnsNotUserTurnWhenNotOneOfPlayersUsername(string username)
    {
        _gameRoom = GetNewGameRoom();
        var ack = _gameRoom.Resign(new RequestResignDto()
        {
            Username = username
        });

        if (username.Equals(PlayerWhite) || username.Equals(PlayerBlack))
            Assert.NotEqual(AckTypes.NotUserTurn, ack);
        else
            Assert.Equal(AckTypes.NotUserTurn, ack);
    }

    [Fact]
    public async Task OfferDrawReturnsNotUserTurnWhenNotOneOfPlayersUsername()
    {
        _gameRoom = GetNewGameRoom();
        var ack = await _gameRoom.OfferDraw(new RequestDrawDto()
        {
            Username = "ElonMusk"
        });

        Assert.Equal(AckTypes.NotUserTurn, ack);
    }

    [Fact]
    public async Task OfferDrawReturnsDrawOfferExpiredIfNotAcceptedWithin10Seconds()
    {
        _gameRoom = GetNewGameRoom();
        var ack = await _gameRoom.OfferDraw(new RequestDrawDto()
        {
            Username = PlayerWhite
        });

        Assert.Equal(AckTypes.DrawOfferExpired, ack);
    }

    [Fact]
    public void DrawOfferResponseReturnsSuccessIfDrawPendingAndValidUsername()
    {
        _gameRoom = GetNewGameRoom();
        var ignore = _gameRoom.OfferDraw(new RequestDrawDto()
        {
            Username = PlayerWhite
        });

        var ack = _gameRoom.DrawOfferResponse(new ResponseDrawDto()
        {
            Accept = false,
            Username = PlayerBlack
        });

        Assert.Equal(AckTypes.Success, ack);
    }

    [Fact]
    public void DrawOfferResponseReturnsNotUserTurnIfAcceptingWithInvalidUser()
    {
        _gameRoom = GetNewGameRoom();
        var ignore = _gameRoom.OfferDraw(new RequestDrawDto()
        {
            Username = PlayerWhite
        });

        var ack = _gameRoom.DrawOfferResponse(new ResponseDrawDto()
        {
            Username = PlayerWhite
        });

        Assert.Equal(AckTypes.NotUserTurn, ack);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OfferDrawReturnsDrawOfferDeclinedOrSuccessIfRespondedByTheOpponentWithinTenSeconds(bool accept)
    {
        _gameRoom = GetNewGameRoom();
        var ack = _gameRoom.OfferDraw(new RequestDrawDto()
        {
            Username = PlayerWhite
        });

        _gameRoom.DrawOfferResponse(new ResponseDrawDto()
        {
            Accept = accept,
            Username = PlayerBlack
        });

        Assert.Equal(accept ? AckTypes.Success : AckTypes.DrawOfferDeclined, ack.Result);
    }

    [Fact]
    public void MakeMoveReturnsGameHasFinishedAfterResigning()
    {
        _gameRoom = GetNewGameRoom();
        _gameRoom.Resign(new RequestResignDto()
        {
            Username = PlayerWhite
        });

        var ack = _gameRoom.MakeMove(new MakeMoveDto());

        Assert.Equal(AckTypes.GameHasFinished, ack);
    }

    [Fact]
    public void MakeMoveReturnsInvalidMoveWhenMoveIsInvalid()
    {
        _gameRoom = GetNewGameRoom();
        var ack = _gameRoom.MakeMove(new MakeMoveDto()
        {
            FromSquare = "e2",
            ToSquare = "e2",
            MoveType = (uint) MoveTypes.Normal
        });

        Assert.Equal(AckTypes.InvalidMove, ack);
    }

    [Fact]
    public void MakeMoveReturnsNotUserTurnWhenWrongPlayerMakesMove()
    {
        _gameRoom = GetNewGameRoom();
        var ack = _gameRoom.MakeMove(new MakeMoveDto()
        {
            Username = PlayerBlack,
            FromSquare = "e2",
            ToSquare = "e4",
            MoveType = (uint) MoveTypes.Normal
        });

        Assert.Equal(AckTypes.NotUserTurn, ack);
    }

    [Fact]
    public void MakeMoveReturnsSuccessWhenValidMakeMoveDto()
    {
        _gameRoom = GetNewGameRoom();
        var ack = _gameRoom.MakeMove(new MakeMoveDto()
        {
            Username = PlayerWhite,
            FromSquare = "e2",
            ToSquare = "e4",
            MoveType = (uint) MoveTypes.Normal
        });

        Assert.Equal(AckTypes.Success, ack);
    }

    [Fact]
    public void GetFenReturnsCorrectFenPositionAfterMakeMoves()
    {
        _gameRoom = GetNewGameRoom();
        List<Tuple<string, string>> moves = new List<Tuple<string, string>>()
        {
            new("e2", "e4"),
            new("e7", "e5"),
            new("g1", "f3"),
            new("b8", "c6"),
        };

        PlayBasicMoves(_gameRoom, moves);
        var fenData = _gameRoom.GetFen();

        Assert.Equal("r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R", fenData.ToString().Split(' ')[0]);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void ReceivesXTimeUpdateEventsInXSecondsWhileGameActive(int seconds)
    {
        var timeUpdateEventsCount = 0;
        _gameRoom = GetNewGameRoom();
        _gameRoom.Initialize();
        _gameRoom.MakeMove(new MakeMoveDto()
        {
            Username = PlayerWhite,
            FromSquare = "e2",
            ToSquare = "e4"
        });

        _gameRoom.GameEvent += delegate(GameRoomEventDto dto)
        {
            if (dto.GameEventDto?.Event == GameStreamEvents.TimeUpdate)
                timeUpdateEventsCount++;
        };

        Thread.Sleep(seconds * 1000 + 500); // Adding 500 milliseconds to compensate for function call delays
        Assert.Equal(seconds, timeUpdateEventsCount);
    }

    [Fact]
    public void ReceivesNewFenPositionEventAfterMakeMove()
    {
        var newFenPositionEventsCount = 0;
        _gameRoom = GetNewGameRoom();
        _gameRoom.GameEvent += delegate(GameRoomEventDto dto)
        {
            if (dto.GameEventDto?.Event == GameStreamEvents.NewFenPosition)
                newFenPositionEventsCount++;
        };

        PlayBasicMoves(_gameRoom, new List<Tuple<string, string>>()
        {
            new("e2", "e4"),
            new("e7", "e5"),
        });

        Assert.Equal(2, newFenPositionEventsCount);
    }

    private static void PlayBasicMoves(GameRoom gameRoom, List<Tuple<string, string>> moves)
    {
        var currentUsername = PlayerWhite;
        foreach (var move in moves)
        {
            gameRoom.MakeMove(new MakeMoveDto()
            {
                Username = currentUsername,
                FromSquare = move.Item1,
                ToSquare = move.Item2
            });

            currentUsername = currentUsername.Equals(PlayerWhite) ? PlayerBlack : PlayerWhite;
        }
    }
}
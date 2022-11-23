using System;
using System.Collections.Generic;
using System.Net.Http;
using Application.Logic;
using Application.LogicInterfaces;
using DatabaseClient.Implementations;
using Domain.DTOs;
using Domain.Enums;
using Rudzoft.ChessLib.Fen;
using Xunit;

namespace Application.Tests;

public class GameLogicUnitTests
{
    public IGameLogic GameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()),new ChatLogic());

    [Theory]
    [InlineData("Alice", 0)]
    [InlineData("StockfishAi1", 1)]
    [InlineData(null, 2)]
    public void StartingGameReturnsCorrectResponseDto(string? opponent, ulong id)
    {
        var response = GameLogic.StartGame(new RequestGameDto()
        {
            Username = "Jeff",
            OpponentType = OpponentTypes.Friend,
            Increment = 0,
            Side = GameSides.White,
            OpponentName = opponent,
            Seconds = 600
        });

        var expectedResponse = new ResponseGameDto()
        {
            Success = true,
            IsWhite = true,
            Opponent = opponent ?? "StockfishAI01",
            GameRoom = id,
            Fen = Fen.StartPositionFen
        };

        Assert.Equal(expectedResponse.Success, response.Result.Success);
        Assert.Equal(expectedResponse.Opponent, response.Result.Opponent);
        Assert.Equal(expectedResponse.IsWhite, response.Result.IsWhite);
        Assert.Equal(expectedResponse.GameRoom, response.Result.GameRoom);
        Assert.Equal(expectedResponse.Fen, response.Result.Fen);
    }

    [Fact]
    public void JoinRoomThrowsArgumentExceptionWhenNoRoomFound()
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            GameLogic.JoinGame(new RequestJoinGameDto()
            {
                Username = "Jeff",
                GameRoom = 0
            });
        });
    }

    [Fact]
    public void MakeMoveReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = GameLogic.MakeMove(new MakeMoveDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }

    [Fact]
    public void ResignReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = GameLogic.Resign(new RequestResignDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }

    [Fact]
    public void OfferDrawReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = GameLogic.OfferDraw(new RequestDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }

    [Fact]
    public void DrawOfferResponseReturnsGameNotFoundWhenNoRoomFound()
    {
        var ack = GameLogic.DrawOfferResponse(new ResponseDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
}
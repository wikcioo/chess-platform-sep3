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
    [Theory]
    [InlineData("Alice", 0)]
    [InlineData("StockfishAi1", 1)]
    [InlineData(null, 2)]
    public void StartingGameReturnsCorrectResponseDto(string? opponent, ulong id)
    {
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        var response = gameLogic.StartGame(new RequestGameDto()
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
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        Assert.Throws<KeyNotFoundException>(() =>
        {
            gameLogic.JoinGame(new RequestJoinGameDto()
            {
                Username = "Jeff",
                GameRoom = 0
            });
        });
    }

    [Fact]
    public void MakeMoveReturnsGameNotFoundWhenNoRoomFound()
    {
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        var ack = gameLogic.MakeMove(new MakeMoveDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
    
    [Fact]
    public void ResignReturnsGameNotFoundWhenNoRoomFound()
    {
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        var ack = gameLogic.Resign(new RequestResignDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
    
    [Fact]
    public void OfferDrawReturnsGameNotFoundWhenNoRoomFound()
    {
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        var ack = gameLogic.OfferDraw(new RequestDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
    
    [Fact]
    public void DrawOfferResponseReturnsGameNotFoundWhenNoRoomFound()
    {
        IGameLogic gameLogic = new GameLogic(new StockfishHttpClient(new HttpClient()));
        var ack = gameLogic.DrawOfferResponse(new ResponseDrawDto());
        Assert.True(ack.Result == AckTypes.GameNotFound);
    }
}
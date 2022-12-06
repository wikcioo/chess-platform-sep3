using Application.ChessTimers;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Factories;

namespace Application.GameRoomHandlers;

public class GameRoomHandlerFactory : IGameRoomHandlerFactory
{
    public GameRoomHandler GetGameRoomHandler(string creator, uint timeControlDurationSeconds, uint timeControlIncrementSeconds,
        bool isVisible, OpponentTypes gameType, GameSides gameSide, string? fen = null)
    {
        var gameRoom = new GameRoom(creator, gameType, timeControlDurationSeconds, timeControlIncrementSeconds,
            gameSide, isVisible);
        var game = GameFactory.Create();
        var chessTimer = new ChessTimer(timeControlDurationSeconds, timeControlIncrementSeconds);
        
        return new GameRoomHandler(game, gameRoom, chessTimer, fen);
    }
}
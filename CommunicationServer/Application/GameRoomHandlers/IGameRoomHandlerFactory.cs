using Domain.Enums;

namespace Application.GameRoomHandlers;

public interface IGameRoomHandlerFactory
{
    GameRoomHandler GetGameRoomHandler(string creator, uint timeControlDurationSeconds,
        uint timeControlIncrementSeconds,
        bool isVisible, OpponentTypes gameType, GameSides gameSide, string? fen = null);
}
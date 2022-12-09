using Domain.Enums;

namespace Application.GameRoomHandlers;

public interface IGameRoomHandlerFactory
{
    IGameRoomHandler GetGameRoomHandler(string creator, uint timeControlDurationSeconds,
        uint timeControlIncrementSeconds,
        bool isVisible, OpponentTypes gameType, GameSides gameSide, string? fen = null);
}
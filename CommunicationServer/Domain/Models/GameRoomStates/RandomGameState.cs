using Domain.Enums;

namespace Domain.Models.GameRoomStates;

public class RandomGameState : IGameState
{
    public static readonly GameStateTypes type = GameStateTypes.Random;
}
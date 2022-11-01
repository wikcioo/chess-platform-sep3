using Domain.Enums;

namespace Application.States;

public class RandomGameState : IGameState
{
    public static readonly GameStateTypes type = GameStateTypes.Random;
}
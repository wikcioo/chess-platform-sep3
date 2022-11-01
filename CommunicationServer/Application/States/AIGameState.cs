using Domain.Enums;

namespace Application.States;

public class AiGameState : IGameState
{
    public static readonly GameStateTypes type = GameStateTypes.Ai;

}
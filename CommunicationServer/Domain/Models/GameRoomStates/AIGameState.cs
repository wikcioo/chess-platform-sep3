using Domain.Enums;

namespace Domain.Models.GameRoomStates;

public class AiGameState : IGameState
{
    public static readonly GameStateTypes type = GameStateTypes.Ai;

}
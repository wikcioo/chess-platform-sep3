using Domain.Enums;

namespace Domain.Models.GameRoomStates;

public class FriendGameState : IGameState
{
    public static readonly GameStateTypes type = GameStateTypes.Friend;

}
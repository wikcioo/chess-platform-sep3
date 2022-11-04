using Domain.DTOs;
using Domain.Enums;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    Task<ResponseGameDto> StartGame(RequestGameDto dto);
    IObservable<JoinedGameStreamDto> JoinGame(RequestJoinGameDto dto);    
    Task<AckTypes> MakeMove(MakeMoveDto dto);
    Task<AckTypes> Resign(RequestResignDto dto);
}
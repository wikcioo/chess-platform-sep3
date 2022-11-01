using Domain.DTOs;
using Domain.Enums;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    Task<ResponseGameDto> StartGame(RequestGameDto dto);
    IObservable<ResponseJoinedGameDto> JoinGame(RequestJoinGameDto dto);    
    Task<AckTypes> MakeMove(MakeMoveDto dto);
}
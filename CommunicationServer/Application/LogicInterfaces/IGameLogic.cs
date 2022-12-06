using Domain.DTOs;
using Domain.DTOs.AuthorizedUserEvents;
using Domain.DTOs.GameEvents;
using Domain.Enums;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    Task<ResponseGameDto> StartGame(RequestGameDto dto, bool isRematch = false);
    AckTypes JoinGame(RequestJoinGameDto dto);
    AckTypes SpectateGame(RequestJoinGameDto dto);
    Task<AckTypes> MakeMove(MakeMoveDto dto);
    Task<AckTypes> Resign(RequestResignDto dto);
    Task<AckTypes> OfferDraw(RequestDrawDto dto);
    Task<AckTypes> DrawOfferResponse(ResponseDrawDto dto);
    Task<AckTypes> OfferRematch(RequestRematchDto dto);
    Task<AckTypes> RematchOfferResponse(ResponseRematchDto dto);
    CurrentGameStateDto GetCurrentGameState(ulong gameRoomId);
    IEnumerable<GameRoomDto> GetGameRooms(GameRoomSearchParameters parameters);
    event Action<GameRoomEventDto>? GameEvent;
    event Action<AuthorizedUserEventDto>? AuthUserEvent;
    
}
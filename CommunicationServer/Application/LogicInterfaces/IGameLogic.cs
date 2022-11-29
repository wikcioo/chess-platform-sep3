using Application.Entities;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
using Domain.Enums;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    Task<ResponseGameDto> StartGame(RequestGameDto dto);
    AckTypes JoinGame(RequestJoinGameDto dto);
    Task<AckTypes> MakeMove(MakeMoveDto dto);
    Task<AckTypes> Resign(RequestResignDto dto);
    Task<AckTypes> OfferDraw(RequestDrawDto dto);
    Task<AckTypes> DrawOfferResponse(ResponseDrawDto dto);
    IEnumerable<SpectateableGameRoomDataDto> GetSpectateableGameRoomData();
    IEnumerable<JoinableGameRoomDataDto> GetJoinableGameRoomData(string requesterUsername);
    JoinedGameStreamDto GetCurrentGameState(ulong gameRoomId);
}
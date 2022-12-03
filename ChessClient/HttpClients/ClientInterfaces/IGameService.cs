using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.DTOs.Chat;
using Domain.Enums;
using HttpClients.Implementations;
using Rudzoft.ChessLib.Types;

namespace HttpClients.ClientInterfaces;

public interface IGameService
{
    event Action<CurrentGameStateDto>? StateReceived;
    bool OnWhiteSide { get; set; }
    ulong? GameRoomId { get; set; }
    bool IsDrawOfferPending { get; set; }
    bool IsRematchOfferRequestPending { get; set; }
    bool IsRematchOfferResponsePending { get; set; }
    RequestGameDto? RequestedGameDto { get; set; }
    event GameService.StreamUpdate? TimeUpdated;
    event GameService.StreamUpdate? NewFenReceived;
    event GameService.StreamUpdate? ResignationReceived;
    event GameService.StreamUpdate? NewPlayerJoined;
    event GameService.StreamUpdate? DrawOffered;
    event GameService.StreamUpdate? DrawOfferTimedOut;
    event GameService.StreamUpdate? DrawOfferAccepted;
    event GameService.StreamUpdate? RematchOffered;
    event GameService.StreamUpdate? RematchOfferTimedOut;
    event GameService.StreamUpdate? RematchOfferAccepted;
    event GameService.StreamUpdate? EndOfTheGameReached;
    event GameService.StreamUpdate? JoinRematchedGame;
    event Action? GameFirstJoined;


    Task<ResponseGameDto> CreateGameAsync(RequestGameDto dto);
    Task JoinGameAsync(RequestJoinGameDto dto);
    Task<AckTypes> MakeMoveAsync(Move move);
    Task<AckTypes> OfferDrawAsync();
    Task<AckTypes> SendDrawResponseAsync(bool accepted);
    Task<AckTypes> OfferRematchAsync();
    Task<AckTypes> SendRematchResponseAsync(bool accepted);
    Task<AckTypes> ResignAsync();
    Task<string> GetLastFenAsync();
    Task<IList<GameRoomDto>> GetGameRoomsAsync(GameRoomSearchParameters parameters);
    Task GetCurrentGameStateAsync();
    void LeaveRoomAsync();
}
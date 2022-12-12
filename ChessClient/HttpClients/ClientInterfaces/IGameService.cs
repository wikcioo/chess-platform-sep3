using Domain.DTOs.Game;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoom;
using Domain.DTOs.StartGame;
using Domain.Enums;
using Rudzoft.ChessLib.Types;

namespace HttpClients.ClientInterfaces;

public interface IGameService
{
    event Action<CurrentGameStateDto>? StateReceived;
    bool OnWhiteSide { get; set; }
    bool Spectating { get; set; }
    ulong? GameRoomId { get; set; }
    bool IsDrawOfferPending { get; set; }
    bool IsRematchOfferRequestPending { get; set; }
    bool IsRematchOfferResponsePending { get; set; }
    RequestGameDto? RequestedGameDto { get; set; }
    event Action<GameEventDto>? TimeUpdated;
    event Action<GameEventDto>? NewFenReceived;
    event Action<GameEventDto>? ResignationReceived;
    event Action<GameEventDto>? NewPlayerJoined;
    event Action<GameEventDto>? DrawOffered;
    event Action<GameEventDto>? DrawOfferTimedOut;
    event Action<GameEventDto>? DrawOfferAccepted;
    event Action<GameEventDto>? RematchOffered;
    event Action<GameEventDto>? RematchOfferTimedOut;
    event Action<GameEventDto>? RematchOfferAccepted;
    event Action<GameEventDto>? EndOfTheGameReached;
    event Action<GameEventDto>? JoinRematchedGame;
    event Action<GameEventDto>? GameAborted;
    event Action? GameFirstJoined;

    Task<ResponseGameDto> CreateGameAsync(RequestGameDto dto);
    Task JoinGameAsync(RequestJoinGameDto dto);
    Task SpectateGameAsync(RequestJoinGameDto dto);
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
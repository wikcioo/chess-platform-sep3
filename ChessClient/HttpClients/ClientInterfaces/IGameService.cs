using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.DTOs.Chat;
using Domain.Enums;
using HttpClients.Implementations;
using Rudzoft.ChessLib.Types;

namespace HttpClients.ClientInterfaces;

public interface IGameService
{
    public event Action<CurrentGameStateDto>? StateReceived;
    bool OnWhiteSide { get; set; }
    ulong? GameRoomId { get; set; }
    bool IsDrawOfferPending { get; set; }
    bool IsRematchOfferRequestPending { get; set; }
    bool IsRematchOfferResponsePending { get; set; }
    public RequestGameDto? RequestedGameDto { get; set; }
    public event GameService.StreamUpdate? TimeUpdated;
    public event GameService.StreamUpdate? NewFenReceived;
    public event GameService.StreamUpdate? ResignationReceived;
    public event GameService.StreamUpdate? NewPlayerJoined;
    public event GameService.StreamUpdate? DrawOffered;
    public event GameService.StreamUpdate? DrawOfferTimedOut;
    public event GameService.StreamUpdate? DrawOfferAccepted;
    public event GameService.StreamUpdate? RematchOffered;
    public event GameService.StreamUpdate? RematchOfferTimedOut;
    public event GameService.StreamUpdate? RematchOfferAccepted;
    public event GameService.StreamUpdate? EndOfTheGameReached;
    public event GameService.StreamUpdate? JoinRematchedGame;
    public event Action? GameFirstJoined;


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
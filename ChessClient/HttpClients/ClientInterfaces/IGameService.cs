using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoomData;
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
    public event GameService.StreamUpdate? TimeUpdated;
    public event GameService.StreamUpdate? NewFenReceived;
    public event GameService.StreamUpdate? ResignationReceived;
    public event GameService.StreamUpdate? NewPlayerJoined;
    public event GameService.StreamUpdate? DrawOffered;
    public event GameService.StreamUpdate? DrawOfferTimedOut;
    public event GameService.StreamUpdate? DrawOfferAccepted;
    public event GameService.StreamUpdate? EndOfTheGameReached;
    public event Action? GameFirstJoined;

    public Task<ResponseGameDto> CreateGame(RequestGameDto dto);
    public Task JoinGame(RequestJoinGameDto dto);
    public Task<AckTypes> MakeMove(Move move);
    public Task<AckTypes> OfferDraw();
    public Task<AckTypes> Resign();
    public Task<AckTypes> SendDrawResponse(bool accepted);
    public Task<IList<SpectateableGameRoomDataDto>> GetAllSpectateableGames();
    public Task<IList<JoinableGameRoomDataDto>> GetAllJoinableGames();
    Task GetCurrentGameState();
    void LeaveRoom();
}
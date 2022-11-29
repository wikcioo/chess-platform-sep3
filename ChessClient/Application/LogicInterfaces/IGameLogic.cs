using Application.LogicImplementations;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using Rudzoft.ChessLib.Types;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    bool OnWhiteSide { get; set; }
    ulong? GameRoomId { get; }
    bool IsDrawOfferPending { get; set; }
    public event GameLogic.StreamUpdate? TimeUpdated;
    public event GameLogic.StreamUpdate? NewFenReceived;
    public event GameLogic.StreamUpdate? ResignationReceived;
    public event GameLogic.StreamUpdate? InitialTimeReceived;
    public event GameLogic.StreamUpdate? DrawOffered;
    public event GameLogic.StreamUpdate? DrawOfferTimedOut;
    public event GameLogic.StreamUpdate? DrawOfferAccepted;
    public event GameLogic.StreamUpdate? EndOfTheGameReached;
    public event GameLogic.StreamUpdate? GameFirstJoined;

    public Task<ResponseGameDto> CreateGame(RequestGameDto dto);
    public Task JoinGame(RequestJoinGameDto dto);
    public Task<int> MakeMove(Move move);
    public Task<AckTypes> OfferDraw();
    public Task Resign();
    public Task<AckTypes> SendDrawResponse(bool accepted);
    public Task<IList<SpectateableGameRoomDataDto>> GetAllSpectateableGames();
    public Task<IList<JoinableGameRoomDataDto>> GetAllJoinableGames();
    void GetCurrentGameState();
    void LeaveRoom();
}
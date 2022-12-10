using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;

namespace Application.GameRoomHandlers;

public interface IGameRoomHandler
{
    bool GameIsActive { get; set; }
    bool FirstMovePlayed { get; set; }
    ulong Id { get; set; }
    public GameOutcome GameOutcome { get; set; }
    GameRoom GameRoom { get; }
    bool IsJoinable { get; set; }
    bool IsSpectateable { get; }
    uint NumPlayersJoined { get; set; }
    uint NumSpectatorsJoined { get; set; }
    string? CurrentPlayer { get; }
    uint GetInitialTimeControlSeconds { get; }
    uint GetInitialTimeControlIncrement { get; }
    event Action<GameRoomEventDto>? GameEvent;
    event Action<GameCreationDto>? GameFinished;
    void Initialize();
    CurrentGameStateDto GetCurrentGameState();
    void PlayerJoined();
    void SendNewGameRoomIdToPlayers(ulong id);
    AckTypes MakeMove(MakeMoveDto dto);
    FenData GetFen();
    AckTypes Resign(RequestResignDto dto);
    Task<AckTypes> OfferDraw(RequestDrawDto dto);
    AckTypes DrawOfferResponse(ResponseDrawDto dto);
    Task<AckTypes> OfferRematch(RequestRematchDto dto);
    AckTypes RematchOfferResponse(ResponseRematchDto dto);
    Move UciMoveToRudzoftMove(string uci);
    GameRoomDto GetGameRoomData();
    bool CanUsernameJoin(string username);
    bool PlayerWhiteJoined { get; set; }
    bool PlayerBlackJoined { get; set; }
}
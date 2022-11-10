using System.Text.RegularExpressions;
using Application.ClientInterfaces;
using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;
using StockfishWrapper;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private ulong _nextGameId;
    private readonly Dictionary<ulong, GameRoom> _gameRooms = new();
    private IStockfishService _stockfishService;

    public GameLogic(IStockfishService stockfishService)
    {
        _stockfishService = stockfishService;
        _nextGameId = 0;
    }

    public Task<ResponseGameDto> StartGame(RequestGameDto dto)
    {
        ResponseGameDto responseDto = new()
        {
            Success = true,
            GameRoom = _nextGameId,
            Opponent = dto.Opponent ?? "StockfishAI01",
            Fen = Fen.StartPositionFen,
            IsWhite = dto.IsWhite ?? true
        };

        var gameType = dto.GameType switch
        {
            "AI" => GameStateTypes.Ai,
            "Friend" => GameStateTypes.Friend,
            "Random" => GameStateTypes.Random,
            _ => throw new Exception(
                "Invalid Game type exception")
        };

        var playerWhite = responseDto.IsWhite ? dto.Username : dto.Opponent;
        var playerBlack = responseDto.IsWhite ? dto.Opponent : dto.Username;
        GameRoom gameRoom = new(playerWhite, playerBlack, dto.Seconds, dto.Increment);
        
        _gameRooms.Add(_nextGameId, gameRoom);
        
        if(gameRoom.CurrentPlayer != null && IsAi(gameRoom.CurrentPlayer))
            RequestAiMove(_nextGameId);

        _nextGameId++;

        return Task.FromResult(responseDto);
    }

    public IObservable<JoinedGameStreamDto> JoinGame(RequestJoinGameDto dto)
    {
        if (_gameRooms.ContainsKey(dto.GameRoom))
        {
            GameRoom gameRoom = _gameRooms[dto.GameRoom];
            return gameRoom.GetMovesAsObservable();
        }

        throw new ArgumentException("Game not found");
    }

    public Task<AckTypes> MakeMove(MakeMoveDto dto)
    {
        if (!_gameRooms.ContainsKey(dto.GameRoom))
            return Task.FromResult(AckTypes.GameNotFound);

        GameRoom room = _gameRooms[dto.GameRoom];

        AckTypes ack = room.MakeMove(dto);

        if (ack != AckTypes.Success)
        {
            return Task.FromResult(ack);
        }
        
        if (room.CurrentPlayer != null && IsAi(room.CurrentPlayer))
        {
            Task.Run(() => RequestAiMove(dto.GameRoom));
        }
        return Task.FromResult(ack);
    }
    
    private static bool IsAi(string? playerName) => StockfishLevels.IsAi(playerName);
    private async void RequestAiMove(ulong roomId)
    {
        
        var room = _gameRooms[roomId];

        if (!IsAi(room.CurrentPlayer))
            throw new InvalidOperationException("Current player is not an AI");

        var uci =  await _stockfishService.GetBestMoveAsync(new StockfishBestMoveDto(room.GetFen().ToString(), room.CurrentPlayer));
        var move =  room.UciMoveToRudzoftMove(uci);
        var dto = new MakeMoveDto
        {
            GameRoom = roomId,
            Username = room.CurrentPlayer!,
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            MoveType = (uint)move.MoveType(),
            Promotion = (uint)move.PromotedPieceType()
        };
        await MakeMove(dto);
    }

    public Task<AckTypes> Resign(RequestResignDto dto)
    {
        if (!_gameRooms.ContainsKey(dto.GameRoom))
            return Task.FromResult(AckTypes.GameNotFound);

        return Task.FromResult(_gameRooms[dto.GameRoom].Resign(dto));
    }

    public async Task<AckTypes> OfferDraw(RequestDrawDto dto)
    {
        if (!_gameRooms.ContainsKey(dto.GameRoom))
            return await Task.FromResult(AckTypes.GameNotFound);

        return await _gameRooms[dto.GameRoom].OfferDraw(dto);
    }

    public Task<AckTypes> DrawOfferResponse(ResponseDrawDto dto)
    {
        if (!_gameRooms.ContainsKey(dto.GameRoom))
            return Task.FromResult(AckTypes.GameNotFound);
        
        return Task.FromResult(_gameRooms[dto.GameRoom].DrawOfferResponse(dto));
    }
}
using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Enums;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private ulong _nextGameId;
    private readonly Dictionary<ulong, GameRoom> _gameRooms = new();

    public Task<ResponseGameDto> StartGame(RequestGameDto dto)
    {
        ResponseGameDto responseDto = new()
        {
            Success = true,
            GameRoom = _nextGameId,
            Opponent = dto.Opponent ?? "StockfishAI",
            Fen = Fen.StartPositionFen,
            IsWhite = dto.IsWhite ?? true
        };

        var gameType = dto.GameType switch
        {
            "AI" => GameStateTypes.Ai,
            "Friend" => GameStateTypes.Friend,
            "Random" => GameStateTypes.Random,
            _ => throw new Exception("Invalid Game type exception")
        };

        GameRoom gameRoom = new(gameType, dto.Seconds, dto.Increment);
        gameRoom.PlayerWhite = responseDto.IsWhite ? dto.Username : dto.Opponent;
        gameRoom.PlayerBlack = responseDto.IsWhite ? dto.Opponent : dto.Username;

        _gameRooms.Add(_nextGameId++, gameRoom);

        return Task.FromResult(responseDto);
    }

    public IObservable<JoinedGameStreamDto> JoinGame(RequestJoinGameDto dto)
    {
        if (_gameRooms.ContainsKey(dto.GameRoom))
        {
            GameRoom gameRoom = _gameRooms[dto.GameRoom];
            return gameRoom.GetMovesAsObservable();
        }

        throw new Exception("Game not found");
    }

    public Task<AckTypes> MakeMove(MakeMoveDto dto)
    {
        if (!_gameRooms.ContainsKey(dto.GameRoom))
            return Task.FromResult(AckTypes.GameNotFound);

        return Task.FromResult(_gameRooms[dto.GameRoom].MakeMove(dto));
    }
}
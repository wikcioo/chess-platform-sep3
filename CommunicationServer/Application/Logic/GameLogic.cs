using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private ulong _nextGameId;
    private readonly Dictionary<ulong, GameRoom> _gameRooms = new();

    public Task<ResponseGameDto> StartGame(RequestGameDto dto){
        ResponseGameDto responseDto = new(){
            Success=true,
            GameRoom = _nextGameId,
            Opponent = dto.Opponent ?? "StockfishAI",
            Fen = Fen.StartPositionFen,
            IsWhite = dto.IsWhite??true
        };
        
        var gameType = dto.GameType switch {
            "AI" => GameStateTypes.Ai,
            "Friend" => GameStateTypes.Friend,
            "Random" => GameStateTypes.Random,
            _ => throw new Exception("Invalid Game type exception") 
        };

        GameRoom gameRoom = new(gameType){
            Seconds = dto.Seconds,
            Increment = dto.Increment
        };
        
        _gameRooms.Add(_nextGameId++, gameRoom);
        return Task.FromResult(responseDto);
    }

    public IObservable<MoveMadeDto> JoinGame(RequestJoinGameDto dto){
        
        if(_gameRooms.ContainsKey(dto.GameRoom))
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
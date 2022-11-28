using Application.ClientInterfaces;
using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using Domain.Models;
using Microsoft.VisualBasic.CompilerServices;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private readonly GameRoomsData _gameRoomsData = new();
    private readonly IStockfishService _stockfishService;
    private readonly IChatLogic _chatLogic;

    public GameLogic(IStockfishService stockfishService, IChatLogic chatLogic)
    {
        _stockfishService = stockfishService;
        _chatLogic = chatLogic;
    }

    public Task<ResponseGameDto> StartGame(RequestGameDto dto)
    {
        GameRoom gameRoom = new(dto.Seconds, dto.Increment, dto.IsVisible, dto.OpponentType)
        {
            GameSide = dto.Side
        };

        var requesterIsWhite = true;
        switch (dto.Side)
        {
            case GameSides.White:
            {
                gameRoom.PlayerWhite = dto.Username;
                gameRoom.PlayerBlack = dto.OpponentName;
                requesterIsWhite = true;
                break;
            }
            case GameSides.Black:
            {
                gameRoom.PlayerWhite = dto.OpponentName;
                gameRoom.PlayerBlack = dto.Username;
                requesterIsWhite = false;
                break;
            }
            case GameSides.Random:
            {
                if (new Random().Next(100) <= 50)
                {
                    gameRoom.PlayerWhite = dto.Username;
                    gameRoom.PlayerBlack = dto.OpponentName;
                    requesterIsWhite = true;
                }
                else
                {
                    gameRoom.PlayerWhite = dto.OpponentName;
                    gameRoom.PlayerBlack = dto.Username;
                    requesterIsWhite = false;
                }

                break;
            }
        }

        gameRoom.Initialize();
        var id = _gameRoomsData.Add(gameRoom);
        _chatLogic.StartChatRoom(id);

        if (dto.OpponentType == OpponentTypes.Ai)
            gameRoom.NumPlayersJoined++;

        if (gameRoom.CurrentPlayer != null && IsAi(gameRoom.CurrentPlayer))
            RequestAiMove(id);

        ResponseGameDto responseDto = new()
        {
            Success = true,
            GameRoom = id,
            Opponent = dto.OpponentName ?? "StockfishAI01",
            Fen = Fen.StartPositionFen,
            IsWhite = requesterIsWhite,
        };

        return Task.FromResult(responseDto);
    }

    public IObservable<JoinedGameStreamDto> JoinGame(RequestJoinGameDto dto)
    {
        var gameRoom = _gameRoomsData.Get(dto.GameRoom);
        if ((gameRoom.IsJoinable && _gameRoomsData.CanUsernameJoin(gameRoom, dto.Username)))
        {
            if (string.IsNullOrEmpty(gameRoom.PlayerWhite) && !dto.Username.Equals(gameRoom.PlayerBlack))
            {
                gameRoom.PlayerWhite = dto.Username;
            }
            else if (string.IsNullOrEmpty(gameRoom.PlayerBlack) && !dto.Username.Equals(gameRoom.PlayerWhite))
            {
                gameRoom.PlayerBlack = dto.Username;
            }

            if (++gameRoom.NumPlayersJoined == 2)
            {
                gameRoom.IsJoinable = false;
                gameRoom.Initialize();
            }

            return gameRoom.GetMovesAsObservable();
        }

        if (gameRoom.IsSpectatable)
        {
            gameRoom.NumSpectatorsJoined++;
            return gameRoom.GetMovesAsObservable();
        }

        throw new ArgumentException("Cannot join the game!");
    }

    public Task<AckTypes> MakeMove(MakeMoveDto dto)
    {
        try
        {
            var room = _gameRoomsData.Get(dto.GameRoom);

            var ack = room.MakeMove(dto);
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
        catch (KeyNotFoundException e)
        {
            return Task.FromResult(AckTypes.GameNotFound);
        }
    }

    private static bool IsAi(string? playerName) => StockfishLevels.IsAi(playerName);

    private async void RequestAiMove(ulong roomId)
    {
        var room = _gameRoomsData.Get(roomId);

        if (!IsAi(room.CurrentPlayer))
            throw new InvalidOperationException("Current player is not an AI");

        var uci = await _stockfishService.GetBestMoveAsync(new StockfishBestMoveDto(room.GetFen().ToString(),
            room.CurrentPlayer));
        var move = room.UciMoveToRudzoftMove(uci);
        var dto = new MakeMoveDto
        {
            GameRoom = roomId,
            Username = room.CurrentPlayer!,
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            MoveType = (uint) move.MoveType(),
            Promotion = (uint) move.PromotedPieceType()
        };
        await MakeMove(dto);
    }

    public Task<AckTypes> Resign(RequestResignDto dto)
    {
        try
        {
            var result = Task.FromResult(_gameRoomsData.Get(dto.GameRoom).Resign(dto));
            if (result.Result == AckTypes.Success)
            {
                _gameRoomsData.Remove(dto.GameRoom);
            }

            return result;
        }
        catch (KeyNotFoundException e)
        {
            return Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public async Task<AckTypes> OfferDraw(RequestDrawDto dto)
    {
        try
        {
            return await _gameRoomsData.Get(dto.GameRoom).OfferDraw(dto);
        }
        catch (KeyNotFoundException e)
        {
            return await Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public Task<AckTypes> DrawOfferResponse(ResponseDrawDto dto)
    {
        try
        {
            var result = Task.FromResult(_gameRoomsData.Get(dto.GameRoom).DrawOfferResponse(dto));
            if (result.Result == AckTypes.Success && dto.Accept)
            {
                _gameRoomsData.Remove(dto.GameRoom);
            }

            return result;
        }
        catch (KeyNotFoundException e)
        {
            return Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public IEnumerable<SpectateableGameRoomDataDto> GetSpectateableGameRoomData()
    {
        IList<SpectateableGameRoomDataDto> list = new List<SpectateableGameRoomDataDto>();
        foreach (var room in _gameRoomsData.GetSpectateable())
        {
            list.Add(new SpectateableGameRoomDataDto()
            {
                GameRoom = room.Id,
                UsernameWhite = room.PlayerWhite!,
                UsernameBlack = room.PlayerBlack!,
                Seconds = room.GetInitialTimeControlSeconds,
                Increment = room.GetInitialTimeControlIncrement
            });
        }

        return list;
    }

    public IEnumerable<JoinableGameRoomDataDto> GetJoinableGameRoomData(string requesterUsername)
    {
        IList<JoinableGameRoomDataDto> list = new List<JoinableGameRoomDataDto>();
        foreach (var room in _gameRoomsData.GetJoinableByUsername(requesterUsername))
        {
            var username = string.IsNullOrEmpty(room.PlayerWhite)
                ? room.PlayerBlack!
                : room.PlayerWhite!;
            list.Add(new JoinableGameRoomDataDto()
            {
                GameRoom = room.Id,
                Username = username,
                Seconds = room.GetInitialTimeControlSeconds,
                Increment = room.GetInitialTimeControlIncrement,
                Side = room.GameSide
            });
        }

        return list;
    }
}
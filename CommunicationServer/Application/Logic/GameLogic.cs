using Application.ClientInterfaces;
using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private readonly GameRoomsData _gameRoomsData = new();
    private readonly IStockfishService _stockfishService;
    private readonly IChatLogic _chatLogic;
    private readonly IUserService _userService;
    public event Action<GameRoomEventDto>? GameEvent;


    public GameLogic(IStockfishService stockfishService, IChatLogic chatLogic, IUserService userService)
    {
        _stockfishService = stockfishService;
        _chatLogic = chatLogic;
        _userService = userService;
    }

    private void FireEvent(GameRoomEventDto dto)
    {
        GameEvent?.Invoke(dto);
    }

    public async Task<ResponseGameDto> StartGame(RequestGameDto dto)
    {
        try
        {
            await ValidateGameRequest(dto);
        }
        catch (InvalidOperationException e)
        {
            ResponseGameDto responseFail = new()
            {
                Success = false,
                ErrorMessage = e.Message
            };
            return responseFail;
        }

        GameRoom gameRoom = new(dto.Seconds, dto.Increment, dto.IsVisible, dto.OpponentType)
        {
            GameSide = dto.Side
        };
        gameRoom.GameEvent += FireEvent;
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

        var id = _gameRoomsData.Add(gameRoom);
        gameRoom.Initialize();
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

        return responseDto;
    }

    private async Task ValidateGameRequest(RequestGameDto dto)
    {
        ValidateTimeControl(dto.Seconds, dto.Increment);

        if (!await ValidateUserExists(dto.Username))
        {
            throw new InvalidOperationException($"User {dto.Username} does not exist in the database.");
        }

        if (dto.OpponentType == OpponentTypes.Friend)
        {
            if (dto.OpponentName == dto.Username)
            {
                throw new InvalidOperationException("Player cannot play against themselves");
            }

            if (IsAi(dto.OpponentName))
            {
                throw new InvalidOperationException("Opponent is an AI in the not ai game.");
            }

            if (dto.OpponentName == null || !await ValidateUserExists(dto.OpponentName))
            {
                throw new InvalidOperationException("Opponent incorrect.");
            }
        }

        if (dto.OpponentType == OpponentTypes.Ai && !IsAi(dto.OpponentName))
            throw new InvalidOperationException("Opponent not an AI in the ai game.");

        if (dto.OpponentType == OpponentTypes.Random)
        {
            if (dto.OpponentName != string.Empty)
            {
                throw new InvalidOperationException("Opponent cannot be chosen for a random game.");
            }

            if (dto.Side != GameSides.Random)
            {
                throw new InvalidOperationException("You cannot choose a side in a random game.");
            }
        }
    }

    private static void ValidateTimeControl(uint seconds, uint increment)
    {
        switch (seconds)
        {
            case < 30:
                throw new InvalidOperationException("Game cannot last less than 30 seconds.");
            case > 86_400:
                throw new InvalidOperationException("Game cannot last longer than a day.");
        }

        if (increment > 60)
        {
            throw new InvalidOperationException("Increment cannot be longer than a minute.");
        }
    }

    private async Task<bool> ValidateUserExists(string username)
    {
        var existing = await _userService.GetByUsernameAsync(username);

        return existing != null;
    }

    public AckTypes JoinGame(RequestJoinGameDto dto)
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
                gameRoom.PlayerJoined();
            }

            return AckTypes.Success;
        }

        if (gameRoom.IsSpectatable)
        {
            gameRoom.NumSpectatorsJoined++;
            return AckTypes.Success;
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

    public CurrentGameStateDto GetCurrentGameState(ulong gameRoomId)
    {
        return _gameRoomsData.Get(gameRoomId).GetCurrentGameState();
    }
}
using Application.ClientInterfaces;
using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.AuthorizedUserEvents;
using Domain.DTOs.GameEvents;
using Domain.DTOs.Stockfish;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private static ulong _nextGameId = 1;

    private readonly Dictionary<ulong, GameRoomHandler> _gameRooms = new();

    private readonly IStockfishService _stockfishService;
    private readonly IChatLogic _chatLogic;
    private readonly IUserService _userService;
    public event Action<GameRoomEventDto>? GameEvent;
    public event Action<AuthorizedUserEventDto>? AuthUserEvent;

    public GameLogic(IStockfishService stockfishService, IChatLogic chatLogic, IUserService userService)
    {
        _stockfishService = stockfishService;
        _chatLogic = chatLogic;
        _userService = userService;
    }

    private void FireGameRoomEvent(GameRoomEventDto dto)
    {
        GameEvent?.Invoke(dto);
    }

    private void FireAuthUserEvent(AuthorizedUserEventDto dto)
    {
        AuthUserEvent?.Invoke(dto);
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

        GameRoomHandler gameRoomHandler =
            new(dto.Username, dto.DurationSeconds, dto.IncrementSeconds, dto.IsVisible, dto.OpponentType);

        gameRoomHandler.GameEvent += FireGameRoomEvent;

        var requesterIsWhite = true;
        switch (dto.Side)
        {
            case GameSides.White:
            {
                gameRoomHandler.PlayerWhite = dto.Username;
                gameRoomHandler.PlayerBlack = dto.OpponentName;
                requesterIsWhite = true;
                break;
            }
            case GameSides.Black:
            {
                gameRoomHandler.PlayerWhite = dto.OpponentName;
                gameRoomHandler.PlayerBlack = dto.Username;
                requesterIsWhite = false;
                break;
            }
            case GameSides.Random:
            {
                if (new Random().Next(100) <= 50)
                {
                    gameRoomHandler.PlayerWhite = dto.Username;
                    gameRoomHandler.PlayerBlack = dto.OpponentName;
                    requesterIsWhite = true;
                }
                else
                {
                    gameRoomHandler.PlayerWhite = dto.OpponentName;
                    gameRoomHandler.PlayerBlack = dto.Username;
                    requesterIsWhite = false;
                }

                break;
            }
        }

        var id = _nextGameId++;
        gameRoomHandler.Id = id;
        _gameRooms.Add(id, gameRoomHandler);
        gameRoomHandler.Initialize();
        _chatLogic.StartChatRoom(id);

        if (dto.OpponentType == OpponentTypes.Ai)
            gameRoomHandler.NumPlayersJoined++;

        if (gameRoomHandler.CurrentPlayer != null && IsAi(gameRoomHandler.CurrentPlayer))
            await RequestAiMove(id);

        if (dto.OpponentType == OpponentTypes.Friend)
        {
            FireAuthUserEvent(new AuthorizedUserEventDto()
            {
                Event = AuthUserEvents.NewGameOffer,
                SenderUsername = dto.Username,
                ReceiverUsername = dto.OpponentName!,
                GameRoomId = id
            });
        }

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
        ValidateTimeControl(dto.DurationSeconds, dto.IncrementSeconds);

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
            throw new InvalidOperationException("IncrementSeconds cannot be longer than a minute.");
        }
    }

    private async Task<bool> ValidateUserExists(string username)
    {
        var existing = await _userService.GetByUsernameAsync(username);

        return existing != null;
    }

    public AckTypes JoinGame(RequestJoinGameDto dto)
    {
        var gameRoom = GetGameRoom(dto.GameRoom);

        if (gameRoom.IsJoinable && gameRoom.CanUsernameJoin(dto.Username))
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

        if (gameRoom.IsSpectateable)
        {
            gameRoom.NumSpectatorsJoined++;
            return AckTypes.Success;
        }

        throw new ArgumentException("Cannot join the game!");
    }

    private GameRoomHandler GetGameRoom(ulong id)
    {
        if (_gameRooms.ContainsKey(id))
            return _gameRooms[id];

        throw new KeyNotFoundException();
    }

    public async Task<AckTypes> MakeMove(MakeMoveDto dto)
    {
        try
        {
            var room = GetGameRoom(dto.GameRoom);

            var ack = room.MakeMove(dto);
            if (ack != AckTypes.Success)
            {
                return ack;
            }

            if (room.CurrentPlayer != null && IsAi(room.CurrentPlayer))
            {
                await RequestAiMove(dto.GameRoom);
            }

            return ack;
        }
        catch (KeyNotFoundException)
        {
            return AckTypes.GameNotFound;
        }
    }

    private static bool IsAi(string? playerName) => StockfishLevels.IsAi(playerName);

    private async Task RequestAiMove(ulong roomId)
    {
        var room = GetGameRoom(roomId);

        if (!IsAi(room.CurrentPlayer))
            throw new InvalidOperationException("Current player is not an AI");

        var uci = await _stockfishService.GetBestMoveAsync(new StockfishBestMoveDto(room.GetFen().ToString(),
            room.CurrentPlayer!));
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
            var result = Task.FromResult(GetGameRoom(dto.GameRoom).Resign(dto));
            if (result.Result == AckTypes.Success)
            {
                _gameRooms.Remove(dto.GameRoom);
            }

            return result;
        }
        catch (KeyNotFoundException)
        {
            return Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public async Task<AckTypes> OfferDraw(RequestDrawDto dto)
    {
        try
        {
            return await GetGameRoom(dto.GameRoom).OfferDraw(dto);
        }
        catch (KeyNotFoundException)
        {
            return await Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public Task<AckTypes> DrawOfferResponse(ResponseDrawDto dto)
    {
        try
        {
            var result = Task.FromResult(GetGameRoom(dto.GameRoom).DrawOfferResponse(dto));
            if (result.Result == AckTypes.Success && dto.Accept)
            {
                _gameRooms.Remove(dto.GameRoom);
            }

            return result;
        }
        catch (KeyNotFoundException)
        {
            return Task.FromResult(AckTypes.GameNotFound);
        }
    }

    private IEnumerable<GameRoomHandler> GetAll()
    {
        return _gameRooms.Select(pair => pair.Value).ToList();
    }

    public IEnumerable<GameRoomDto> GetGameRooms(GameRoomSearchParameters parameters)
    {
        var rooms = GetAll();

        if (parameters.Spectateable)
        {
            rooms = rooms.Where(room => room.IsSpectateable);
        }

        if (parameters.Joinable)
        {
            rooms = rooms.Where(room => room.IsJoinable && room.CanUsernameJoin(parameters.RequesterName));
        }

        return rooms.Select(room => room.GetGameRoomData());
    }

    public CurrentGameStateDto GetCurrentGameState(ulong gameRoomId)
    {
        return GetGameRoom(gameRoomId).GetCurrentGameState();
    }
}
using Application.ClientInterfaces;
using Application.GameRoomHandlers;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.AuthorizedUserEvents;
using Domain.DTOs.GameEvents;
using Domain.DTOs.Stockfish;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;

namespace Application.Logic;

public class GameLogic : IGameLogic
{
    private static ulong _nextGameId = 1;

    private readonly Dictionary<ulong, GameRoomHandler> _gameRooms = new();

    private readonly Dictionary<ulong, GameRoomHandler> _tempGameRoomsData = new();

    private readonly IStockfishService _stockfishService;
    private readonly IChatLogic _chatLogic;
    private readonly IUserService _userService;
    private readonly IGameRoomHandlerFactory _gameRoomHandlerFactory;
    private readonly IGameService _gameService;
    public event Action<GameRoomEventDto>? GameEvent;
    public event Action<AuthorizedUserEventDto>? AuthUserEvent;

    public GameLogic(IStockfishService stockfishService, IChatLogic chatLogic, IUserService userService,
        IGameService gameService,
        IGameRoomHandlerFactory gameRoomHandlerFactory)
    {
        _stockfishService = stockfishService;
        _chatLogic = chatLogic;
        _userService = userService;
        _gameRoomHandlerFactory = gameRoomHandlerFactory;
        _gameService = gameService;
    }


    private void FireGameRoomEvent(GameRoomEventDto dto)
    {
        if (dto.GameEventDto?.Event == GameStreamEvents.ReachedEndOfTheGame ||
            dto.GameEventDto is {Event: GameStreamEvents.TimeUpdate, GameEndType: (int)GameEndTypes.TimeIsUp})
        {
            _tempGameRoomsData.Add(dto.GameRoomId, GetGameRoom(dto.GameRoomId, _gameRooms));
            _gameRooms.Remove(dto.GameRoomId);
        }

        GameEvent?.Invoke(dto);
    }

    private void FireAuthUserEvent(AuthorizedUserEventDto dto)
    {
        AuthUserEvent?.Invoke(dto);
    }

    private async void SaveGame(GameCreationDto dto)
    {
        await _gameService.CreateAsync(dto);
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

        var gameRoomHandler = _gameRoomHandlerFactory.GetGameRoomHandler(dto.Username,
            dto.DurationSeconds, dto.IncrementSeconds, dto.IsVisible, dto.OpponentType, dto.Side);

        gameRoomHandler.GameEvent += FireGameRoomEvent;
        gameRoomHandler.GameFinished += SaveGame;

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

        if (dto.OpponentType == OpponentTypes.Friend && !dto.IsRematch)
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

        _ = Task.Run((async () =>
        {
            await Task.Delay(3 * 60000);

            if (!GetGameRoom(id, _gameRooms).FirstMovePlayed ||
                string.IsNullOrEmpty(GetGameRoom(id, _gameRooms).PlayerBlack) ||
                string.IsNullOrEmpty(GetGameRoom(id, _gameRooms).PlayerWhite))
            {
                _gameRooms.Remove(id);
                FireGameRoomEvent(new GameRoomEventDto()
                {
                    GameRoomId = id,
                    GameEventDto = new GameEventDto()
                    {
                        Event = GameStreamEvents.GameAborted
                    }
                });
            }
        }));

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
        var gameRoom = GetGameRoom(dto.GameRoom, _gameRooms);

        // Allow rejoining
        if (gameRoom.PlayerWhite != null && gameRoom.PlayerWhiteJoined && gameRoom.PlayerWhite.Equals(dto.Username) ||
            gameRoom.PlayerBlack != null && gameRoom.PlayerBlackJoined && gameRoom.PlayerBlack.Equals(dto.Username))
        {
            return AckTypes.Success;
        }

        if (!gameRoom.IsJoinable || !gameRoom.CanUsernameJoin(dto.Username))
            throw new ArgumentException("Cannot join the game!");


        if (string.IsNullOrEmpty(gameRoom.PlayerWhite) && !dto.Username.Equals(gameRoom.PlayerBlack))
        {
            gameRoom.PlayerWhite = dto.Username;
        }
        else if (string.IsNullOrEmpty(gameRoom.PlayerBlack) && !dto.Username.Equals(gameRoom.PlayerWhite))
        {
            gameRoom.PlayerBlack = dto.Username;
        }

        if (gameRoom.PlayerWhite != null && gameRoom.PlayerWhite.Equals(dto.Username) &&
            gameRoom.PlayerWhiteJoined == false)
        {
            gameRoom.NumPlayersJoined++;
            gameRoom.PlayerWhiteJoined = true;
        }

        if (gameRoom.PlayerBlack != null && gameRoom.PlayerBlack.Equals(dto.Username) &&
            gameRoom.PlayerBlackJoined == false)
        {
            gameRoom.NumPlayersJoined++;
            gameRoom.PlayerBlackJoined = true;
        }

        if (gameRoom.NumPlayersJoined != 2) return AckTypes.Success;

        gameRoom.IsJoinable = false;
        gameRoom.PlayerJoined();

        return AckTypes.Success;
    }

    public AckTypes SpectateGame(RequestJoinGameDto dto)
    {
        var gameRoom = GetGameRoom(dto.GameRoom, _gameRooms);

        if (gameRoom.IsSpectateable)
        {
            gameRoom.NumSpectatorsJoined++;
            return AckTypes.Success;
        }

        throw new ArgumentException("Cannot spectate the game!");
    }

    private GameRoomHandler GetGameRoom(ulong id, Dictionary<ulong, GameRoomHandler> gameRoomHandlers)
    {
        if (gameRoomHandlers.ContainsKey(id))
            return gameRoomHandlers[id];

        throw new KeyNotFoundException();
    }

    public async Task<AckTypes> MakeMove(MakeMoveDto dto)
    {
        try
        {
            var room = GetGameRoom(dto.GameRoom, _gameRooms);

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
        var room = GetGameRoom(roomId, _gameRooms);

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
            MoveType = (uint)move.MoveType(),
            Promotion = (uint)move.PromotedPieceType()
        };
        await MakeMove(dto);
    }

    public Task<AckTypes> Resign(RequestResignDto dto)
    {
        try
        {
            var result = Task.FromResult(GetGameRoom(dto.GameRoom, _gameRooms).Resign(dto));
            if (result.Result == AckTypes.Success)
            {
                _tempGameRoomsData.Add(dto.GameRoom, GetGameRoom(dto.GameRoom, _gameRooms));
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
            return await GetGameRoom(dto.GameRoom, _gameRooms).OfferDraw(dto);
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
            var result = Task.FromResult(GetGameRoom(dto.GameRoom, _gameRooms).DrawOfferResponse(dto));
            if (result.Result == AckTypes.Success && dto.Accept)
            {
                _tempGameRoomsData.Add(dto.GameRoom, GetGameRoom(dto.GameRoom, _gameRooms));
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

    public async Task<AckTypes> OfferRematch(RequestRematchDto dto)
    {
        try
        {
            return await GetGameRoom(dto.GameRoom, _tempGameRoomsData).OfferRematch(dto);
        }
        catch (KeyNotFoundException)
        {
            return await Task.FromResult(AckTypes.GameNotFound);
        }
    }

    public async Task<AckTypes> RematchOfferResponse(ResponseRematchDto dto)
    {
        try
        {
            var gameRoom = GetGameRoom(dto.GameRoom, _tempGameRoomsData);
            var result = gameRoom.RematchOfferResponse(dto);
            if (result == AckTypes.Success && dto.Accept)
            {
                var res = await StartGame(new RequestGameDto()
                {
                    DurationSeconds = gameRoom.GetInitialTimeControlSeconds,
                    IncrementSeconds = gameRoom.GetInitialTimeControlIncrement,
                    Side = gameRoom.GameSide,
                    Username = dto.Username,
                    IsVisible = gameRoom.IsVisible,
                    OpponentType = OpponentTypes.Friend,
                    OpponentName = gameRoom.PlayerWhite!.Equals(dto.Username)
                        ? gameRoom.PlayerBlack
                        : gameRoom.PlayerWhite,
                    IsRematch = true
                });

                _tempGameRoomsData.Remove(dto.GameRoom);
                gameRoom.SendNewGameRoomIdToPlayers(res.GameRoom);
            }

            return result;
        }
        catch (KeyNotFoundException)
        {
            return AckTypes.GameNotFound;
        }
    }

    public IEnumerable<GameRoomDto> GetGameRooms(GameRoomSearchParameters parameters)
    {
        var rooms = GetAll();

        if (parameters.Spectateable)
        {
            rooms = rooms.Where(room => room.IsSpectateable &&
                                        (room.PlayerWhite != null &&
                                         !room.PlayerWhite.Equals(parameters.RequesterName)) &&
                                        (room.PlayerBlack != null &&
                                         !room.PlayerBlack.Equals(parameters.RequesterName)));
        }
        else if (parameters.Joinable)
        {
            rooms = rooms.Where(room =>
                ((room.IsJoinable && room.CanUsernameJoin(parameters.RequesterName)) ||
                 (room.PlayerWhite != null && room.PlayerWhite.Equals(parameters.RequesterName) ||
                  (room.PlayerBlack != null && room.PlayerBlack.Equals(parameters.RequesterName)))));
        }

        return rooms.Select(room => room.GetGameRoomData());
    }

    public CurrentGameStateDto GetCurrentGameState(ulong gameRoomId)
    {
        return GetGameRoom(gameRoomId, _gameRooms).GetCurrentGameState();
    }
}
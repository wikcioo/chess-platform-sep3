using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Application.Entities;

public class GameRoomHandler
{
    private readonly IGame _game;
    private readonly ChessTimer _chessTimer;
    private bool _whitePlaying;
    private bool _firstMovePlayed;
    public bool GameIsActive = false;

    // Offer draw related fields
    private string _drawOfferOrigin = string.Empty;
    private bool _isDrawOffered = false;
    private bool _isDrawOfferAccepted = false;
    private bool _drawResponseWithinTimespan = false;
    private CancellationTokenSource? _drawCts;

    // Offer rematch related fields
    private string _rematchOfferOrigin = string.Empty;
    private bool _isRematchOffered = false;
    private bool _isRematchOfferAccepted = false;
    private bool _rematchResponseWithinTimespan = false;
    private CancellationTokenSource? _rematchCts;

    public event Action<GameRoomEventDto>? GameEvent;

    public ulong Id { get; set; }

    private readonly GameRoom _gameRoom;

    private string Creator => _gameRoom.Creator;

    public string? PlayerWhite
    {
        get => _gameRoom.PlayerWhite;
        set => _gameRoom.PlayerWhite = value;
    }

    public string? PlayerBlack
    {
        get => _gameRoom.PlayerBlack;
        set => _gameRoom.PlayerBlack = value;
    }

    public GameSides GameSide => _gameRoom.GameSide;

    private OpponentTypes GameType => _gameRoom.GameType;
    public bool IsVisible { get; set; }
    public bool IsJoinable { get; set; } = true;
    public bool IsSpectateable => IsVisible && !IsJoinable;

    public uint NumPlayersJoined { get; set; }
    public uint NumSpectatorsJoined { get; set; }

    public string? CurrentPlayer => _game.CurrentPlayer() == Player.White ? PlayerWhite : PlayerBlack;
    public uint GetInitialTimeControlSeconds => _gameRoom.TimeControlDurationSeconds;
    public uint GetInitialTimeControlIncrement => _gameRoom.TimeControlIncrementSeconds;

    public GameRoomHandler(string creator, uint timeControlDurationSeconds, uint timeControlIncrementSeconds,
        bool isVisible, OpponentTypes gameType, GameSides gameSide, string? fen = null)
    {
        _gameRoom = new GameRoom(creator, gameType, timeControlDurationSeconds, timeControlIncrementSeconds, gameSide);
        _game = GameFactory.Create();
        _game.NewGame(fen ?? Fen.StartPositionFen);
        _chessTimer = new ChessTimer(_whitePlaying, timeControlDurationSeconds, timeControlIncrementSeconds);
        IsVisible = isVisible;
        _whitePlaying = _game.CurrentPlayer().IsWhite;
        if (gameType == OpponentTypes.Ai)
        {
            GameIsActive = true;
        }
    }

    public void Initialize()
    {
        _chessTimer.ThrowEvent += (_, _, dto) =>
        {
            if (dto.GameEndType == (uint) GameEndTypes.TimeIsUp) GameIsActive = false;
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = dto
            });
        };
    }

    public CurrentGameStateDto GetCurrentGameState()
    {
        var stateDto = new CurrentGameStateDto
        {
            FenString = _game.Pos.FenNotation,
            WhiteTimeLeftMs = _chessTimer.WhiteRemainingTimeMs,
            BlackTimeLeftMs = _chessTimer.BlackRemainingTimeMs,
            UsernameWhite = PlayerWhite ?? "",
            UsernameBlack = PlayerBlack ?? "",
            IsWhite = _whitePlaying
        };

        return stateDto;
    }

    public void PlayerJoined()
    {
        GameIsActive = true;
        var streamDto = new GameEventDto
        {
            FenString = _game.Pos.FenNotation,
            Event = GameStreamEvents.PlayerJoined,
            TimeLeftMs = _chessTimer.TimeControlDurationMs,
            UsernameWhite = PlayerWhite ?? "",
            UsernameBlack = PlayerBlack ?? ""
        };
        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });
    }

    public void SendNewGameRoomIdToPlayers(ulong id)
    {
        GameEvent?.Invoke(new GameRoomEventDto()
        {
            GameRoomId = Id,
            GameEventDto = new GameEventDto()
            {
                Event = GameStreamEvents.RematchInvitation,
                GameRoomId = id
            }
        });
    }

    public AckTypes MakeMove(MakeMoveDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;

        if (!dto.Username.Equals(CurrentPlayer))
            return AckTypes.NotUserTurn;

        var move = ParseMove(dto);

        if (!IsValidMove(move))
            return AckTypes.InvalidMove;

        if (!_firstMovePlayed)
        {
            _chessTimer.StartTimers();
            _firstMovePlayed = true;
        }


        _game.Pos.MakeMove(move, _game.Pos.State);
        _chessTimer.UpdateTimers(_whitePlaying);

        // Check game logic: Checkmate, Draw, Insufficient Material
        // TODO(Wiktor): Implement the rest of end game scenarios
        var reachedTheEnd = false;
        var gameEndType = GameEndTypes.None;
        if (_game.Pos.IsMate)
        {
            reachedTheEnd = true;
            gameEndType = GameEndTypes.CheckMate;
        }
        else if (_game.Pos.GenerateMoves().Length == 0 && !_game.Pos.InCheck)
        {
            reachedTheEnd = true;
            gameEndType = GameEndTypes.Pat;
        }

        _whitePlaying = _game.CurrentPlayer().IsWhite;

        if (reachedTheEnd)
        {
            GameIsActive = false;
            _chessTimer.StopTimers();
            var streamDto = new GameEventDto()
            {
                Event = GameStreamEvents.ReachedEndOfTheGame,
                FenString = _game.Pos.FenNotation,
                GameEndType = (uint) gameEndType,
                IsWhite = _whitePlaying,
                TimeLeftMs = _whitePlaying ? _chessTimer.WhiteRemainingTimeMs : _chessTimer.BlackRemainingTimeMs,
            };
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = streamDto
            });

            return AckTypes.Success;
        }


        var responseJoinedGameDto = new GameEventDto()
        {
            Event = GameStreamEvents.NewFenPosition,
            FenString = _game.Pos.FenNotation,
            TimeLeftMs = _whitePlaying ? _chessTimer.WhiteRemainingTimeMs : _chessTimer.BlackRemainingTimeMs,
            GameEndType = (uint) _game.GameEndType,
            IsWhite = _whitePlaying
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = responseJoinedGameDto
        });

        return AckTypes.Success;
    }

    public FenData GetFen()
    {
        return _game.GetFen();
    }

    public AckTypes Resign(RequestResignDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;
        if (!dto.Username.Equals(PlayerWhite) && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        _chessTimer.StopTimers();
        GameIsActive = false;

        var streamDto = new GameEventDto()
        {
            Event = GameStreamEvents.Resignation,
            IsWhite = PlayerWhite!.Equals(dto.Username)
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });

        return AckTypes.Success;
    }

    public async Task<AckTypes> OfferDraw(RequestDrawDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;
        if (!dto.Username.Equals(PlayerWhite) && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        _isDrawOffered = true;
        _drawOfferOrigin = dto.Username;
        _isDrawOfferAccepted = false;
        _drawResponseWithinTimespan = false;

        var streamDto = new GameEventDto
        {
            Event = GameStreamEvents.DrawOffer,
            UsernameWhite = PlayerBlack!.Equals(dto.Username) ? PlayerWhite! : "",
            UsernameBlack = PlayerWhite!.Equals(dto.Username) ? PlayerBlack! : ""
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });

        _drawCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            if (_drawCts.Token.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(50);
        }

        if (!_drawResponseWithinTimespan)
        {
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = new GameEventDto()
                {
                    Event = GameStreamEvents.DrawOfferTimeout,
                    IsWhite = PlayerWhite!.Equals(dto.Username)
                }
            });
            return AckTypes.DrawOfferExpired;
        }

        if (!_isDrawOfferAccepted && _drawResponseWithinTimespan) return AckTypes.DrawOfferDeclined;
        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = new GameEventDto
            {
                Event = GameStreamEvents.DrawOfferAcceptation,
                IsWhite = PlayerWhite!.Equals(dto.Username)
            }
        });
        return AckTypes.Success;
    }

    public AckTypes DrawOfferResponse(ResponseDrawDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;
        if (!_isDrawOffered) return AckTypes.DrawNotOffered;
        if (!dto.Username.Equals(PlayerWhite) && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        if (dto.Username.Equals(_drawOfferOrigin))
        {
            return AckTypes.NotUserTurn;
        }

        _drawResponseWithinTimespan = true;
        if (dto.Accept)
        {
            _isDrawOfferAccepted = dto.Accept;
            _chessTimer.StopTimers();
            GameIsActive = false;
        }

        try
        {
            _drawCts?.Cancel();
        }
        catch (Exception)
        {
            // ignored
        }

        return AckTypes.Success;
    }

    public async Task<AckTypes> OfferRematch(RequestRematchDto dto)
    {
        if (!dto.Username.Equals(PlayerWhite) && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        _isRematchOffered = true;
        _rematchOfferOrigin = dto.Username;
        _isRematchOfferAccepted = false;
        _rematchResponseWithinTimespan = false;

        var streamDto = new GameEventDto
        {
            Event = GameStreamEvents.RematchOffer,
            UsernameWhite = PlayerBlack!.Equals(dto.Username) ? PlayerWhite! : "",
            UsernameBlack = PlayerWhite!.Equals(dto.Username) ? PlayerBlack! : ""
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });

        _rematchCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        while (true)
        {
            if (_rematchCts.Token.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(50);
        }

        if (!_rematchResponseWithinTimespan)
        {
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = new GameEventDto()
                {
                    Event = GameStreamEvents.RematchOfferTimeout,
                    IsWhite = PlayerWhite!.Equals(dto.Username)
                }
            });
            return AckTypes.RematchOfferExpired;
        }

        if (!_isRematchOfferAccepted && _rematchResponseWithinTimespan) return AckTypes.RematchOfferDeclined;
        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = new GameEventDto
            {
                Event = GameStreamEvents.RematchOfferAcceptation,
                IsWhite = PlayerWhite!.Equals(dto.Username)
            }
        });

        return AckTypes.Success;
    }

    public AckTypes RematchOfferResponse(ResponseRematchDto dto)
    {
        if (!_isRematchOffered) return AckTypes.RematchNotOffered;
        if (!dto.Username.Equals(PlayerWhite) && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        if (dto.Username.Equals(_rematchOfferOrigin))
        {
            return AckTypes.NotUserTurn;
        }

        _rematchResponseWithinTimespan = true;
        if (dto.Accept)
        {
            _isRematchOfferAccepted = dto.Accept;
        }

        try
        {
            _rematchCts?.Cancel();
        }
        catch (Exception)
        {
            // ignored
        }

        return AckTypes.Success;
    }

    private bool IsValidMove(Move move)
    {
        return _game.Pos.GenerateMoves().ToList().Any(valid => move.Equals(valid));
    }

    public Move UciMoveToRudzoftMove(string uci)
    {
        if (uci.Length < 4) throw new Exception("UCI move does not have 4 characters!");

        var from = uci[..2];
        var to = uci[2..];

        var fromSq = UciToSquare(from);
        var toSq = UciToSquare(to);

        var moveType = MoveTypes.Normal;
        var promotionPiece = PieceTypes.Knight;

        if (from is "e1" or "e8" && to is "c1" or "g1" or "c8" or "g8")
        {
            to = from switch
            {
                "e1" when to is "c1" => "a1",
                "e1" when to is "g1" => "h1",
                "e8" when to is "c8" => "a8",
                "e8" when to is "g8" => "h8",
                _ => to
            };
            moveType = MoveTypes.Castling;
            toSq = UciToSquare(to);
        }
        else if (uci.Length == 5)
        {
            moveType = MoveTypes.Promotion;
            promotionPiece = GetPromotionPiece(uci[4]);
        }
        else if (_game.Pos.EnPassantSquare.RankChar != '9')
        {
            moveType = MoveTypes.Enpassant;
        }

        var move = new Move(fromSq, toSq, moveType, promotionPiece);
        return move;
    }

    private static Square UciToSquare(string uci)
    {
        var sqNum = (uci[1] - '1') * 8 + (uci[0] - 'a');
        return new Square(sqNum);
    }

    private static PieceTypes GetPromotionPiece(char letter)
    {
        return letter switch
        {
            'q' => PieceTypes.Queen,
            'k' => PieceTypes.Knight,
            'b' => PieceTypes.Bishop,
            'r' => PieceTypes.Rook,
            _ => PieceTypes.Knight
        };
    }

    private static Move ParseMove(MakeMoveDto dto)
    {
        var fromSquare = UciToSquare(dto.FromSquare);
        var toSquare = UciToSquare(dto.ToSquare);
        var moveType = (MoveTypes) (dto.MoveType ?? 0);
        var promotionPiece = (PieceTypes?) dto.Promotion;

        return moveType switch
        {
            MoveTypes.Normal => new Move(fromSquare, toSquare),
            MoveTypes.Castling or
                MoveTypes.Enpassant => new Move(fromSquare, toSquare, moveType),
            MoveTypes.Promotion => new Move(fromSquare, toSquare, moveType, promotionPiece ?? PieceTypes.Queen),
            _ => throw new Exception("Invalid move type value.")
        };
    }

    public GameRoomDto GetGameRoomData()
    {
        return new GameRoomDto()
        {
            GameRoom = Id,
            Creator = Creator,
            UsernameWhite = PlayerWhite!,
            UsernameBlack = PlayerBlack!,
            DurationSeconds = GetInitialTimeControlSeconds,
            IncrementSeconds = GetInitialTimeControlIncrement
        };
    }

    public bool CanUsernameJoin(string username)
    {
        if (GameType is OpponentTypes.Random or OpponentTypes.Ai)
            return true;

        return PlayerWhite!.Equals(username) || PlayerBlack!.Equals(username);
    }
}
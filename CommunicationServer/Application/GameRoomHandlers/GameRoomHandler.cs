using Application.ChessTimers;
using Domain.DTOs.Draw;
using Domain.DTOs.Game;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoom;
using Domain.DTOs.Rematch;
using Domain.DTOs.Resignation;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Application.GameRoomHandlers;

public class GameRoomHandler : IGameRoomHandler
{
    private readonly IGame _game;
    private readonly IChessTimer _chessTimer;
    private bool _whitePlaying;

    public bool FirstMovePlayed { get; set; }
    public bool GameIsActive { get; set; } = false;

    // Offer draw related fields
    private string _drawOfferOrigin = string.Empty;
    private bool _isDrawOffered;
    private bool _isDrawOfferAccepted;
    private bool _drawResponseWithinTimespan;
    private readonly CountDownTimer _drawOfferCountDownTimer = new();

    // Offer rematch related fields
    private string _rematchOfferOrigin = string.Empty;
    private bool _isRematchOffered;
    private bool _isRematchOfferAccepted;
    private bool _rematchResponseWithinTimespan;
    private readonly CountDownTimer _rematchCountDownTimer = new();

    public event Action<GameRoomEventDto>? GameEvent;
    public event Action<GameCreationDto>? GameFinished;

    public ulong Id { get; set; }
    public GameOutcome GameOutcome { get; set; }
    public GameRoom GameRoom { get; }
    public bool IsJoinable { get; set; } = true;
    public bool IsSpectateable => GameRoom.IsVisible && !IsJoinable;

    public uint NumPlayersJoined { get; set; }
    public uint NumSpectatorsJoined { get; set; }

    public string? CurrentPlayer => _game.CurrentPlayer() == Player.White ? GameRoom.PlayerWhite : GameRoom.PlayerBlack;
    public uint GetInitialTimeControlSeconds => GameRoom.TimeControlDurationSeconds;
    public uint GetInitialTimeControlIncrement => GameRoom.TimeControlIncrementSeconds;

    public bool PlayerWhiteJoined { get; set; }
    public bool PlayerBlackJoined { get; set; }

    public GameRoomHandler(IGame game, GameRoom gameRoom, IChessTimer chessTimer, string? fen = null)
    {
        GameRoom = gameRoom;

        _game = game;
        _game.NewGame(fen ?? Fen.StartPositionFen);

        _chessTimer = chessTimer;
        _whitePlaying = _game.CurrentPlayer().IsWhite;
        if (GameRoom.GameType == OpponentTypes.Ai)
        {
            GameIsActive = true;
        }
    }

    public void Initialize()
    {
        _chessTimer.Elapsed += (dto) =>
        {
            if (dto.GameEndType == (uint)GameEndTypes.TimeIsUp) FinishGame();
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
            UsernameWhite = GameRoom.PlayerWhite ?? "",
            UsernameBlack = GameRoom.PlayerBlack ?? "",
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
            UsernameWhite = GameRoom.PlayerWhite ?? "",
            UsernameBlack = GameRoom.PlayerBlack ?? ""
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

        if (!FirstMovePlayed)
        {
            _chessTimer.StartTimers();
            FirstMovePlayed = true;
        }


        _game.Pos.MakeMove(move, _game.Pos.State);
        _chessTimer.UpdateTimers(_whitePlaying);

        var gameEndType = IsEndGame();

        _whitePlaying = _game.CurrentPlayer().IsWhite;

        if (gameEndType != GameEndTypes.None)
        {
            if (gameEndType == GameEndTypes.Pat)
            {
                GameOutcome = GameOutcome.Draw;
            }
            else if (gameEndType == GameEndTypes.CheckMate)
            {
                GameOutcome = GameRoom.PlayerWhite!.Equals(dto.Username) ? GameOutcome.White : GameOutcome.Black;
            }

            FinishGame();
            var streamDto = new GameEventDto()
            {
                Event = GameStreamEvents.ReachedEndOfTheGame,
                FenString = _game.Pos.FenNotation,
                GameEndType = (uint)gameEndType,
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
            GameEndType = (uint)_game.GameEndType,
            IsWhite = _whitePlaying
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = responseJoinedGameDto
        });

        return AckTypes.Success;
    }

    private GameEndTypes IsEndGame()
    {
        // Check game logic: Checkmate, Draw, Insufficient Material
        // TODO(Wiktor): Implement the rest of end game scenarios
        var gameEndType = GameEndTypes.None;
        if (_game.Pos.IsMate)
        {
            gameEndType = GameEndTypes.CheckMate;
        }
        else if (_game.Pos.GenerateMoves().Length == 0 && !_game.Pos.InCheck)
        {
            gameEndType = GameEndTypes.Pat;
        }

        return gameEndType;
    }

    private void FinishGame()
    {
        GameIsActive = false;
        _chessTimer.StopTimers();
        GameFinished?.Invoke(new GameCreationDto
        {
            Creator = GameRoom.Creator,
            PlayerWhite = GameRoom.PlayerWhite,
            PlayerBlack = GameRoom.PlayerBlack,
            GameType = GameRoom.GameType,
            IsVisible = GameRoom.IsVisible,
            TimeControlDurationSeconds = GetInitialTimeControlSeconds,
            TimeControlIncrementSeconds = GetInitialTimeControlIncrement,
            GameOutcome = GameOutcome
        });
    }

    public FenData GetFen()
    {
        return _game.GetFen();
    }

    public AckTypes Resign(RequestResignDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;
        if (!dto.Username.Equals(GameRoom.PlayerWhite) && !dto.Username.Equals(GameRoom.PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        GameOutcome = GameRoom.PlayerWhite!.Equals(dto.Username) ? GameOutcome.Black : GameOutcome.White;
        FinishGame();

        var streamDto = new GameEventDto()
        {
            Event = GameStreamEvents.Resignation,
            IsWhite = GameRoom.PlayerWhite!.Equals(dto.Username)
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
        if (!dto.Username.Equals(GameRoom.PlayerWhite) && !dto.Username.Equals(GameRoom.PlayerBlack))
        {
            return AckTypes.NotUserTurn;
        }

        _isDrawOffered = true;
        _drawOfferOrigin = dto.Username;
        _isDrawOfferAccepted = false;

        var streamDto = new GameEventDto
        {
            Event = GameStreamEvents.DrawOffer,
            UsernameWhite = GameRoom.PlayerBlack!.Equals(dto.Username) ? GameRoom.PlayerWhite! : "",
            UsernameBlack = GameRoom.PlayerWhite!.Equals(dto.Username) ? GameRoom.PlayerBlack! : ""
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });

        _drawResponseWithinTimespan = await _drawOfferCountDownTimer.StartTimer(10);

        if (!_drawResponseWithinTimespan)
        {
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = new GameEventDto()
                {
                    Event = GameStreamEvents.DrawOfferTimeout,
                    IsWhite = GameRoom.PlayerWhite!.Equals(dto.Username)
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
                IsWhite = GameRoom.PlayerWhite!.Equals(dto.Username)
            }
        });
        return AckTypes.Success;
    }

    public AckTypes DrawOfferResponse(ResponseDrawDto dto)
    {
        if (!GameIsActive) return AckTypes.GameHasFinished;
        if (!_isDrawOffered) return AckTypes.DrawNotOffered;
        if (!dto.Username.Equals(GameRoom.PlayerWhite) && !dto.Username.Equals(GameRoom.PlayerBlack))
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
            GameOutcome = GameOutcome.Draw;
            FinishGame();
        }

        try
        {
            _drawOfferCountDownTimer.StopTimer();
        }
        catch (Exception)
        {
            // ignored
        }

        return AckTypes.Success;
    }

    public async Task<AckTypes> OfferRematch(RequestRematchDto dto)
    {
        if (!dto.Username.Equals(GameRoom.PlayerWhite) && !dto.Username.Equals(GameRoom.PlayerBlack))
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
            UsernameWhite = GameRoom.PlayerBlack!.Equals(dto.Username) ? GameRoom.PlayerWhite! : "",
            UsernameBlack = GameRoom.PlayerWhite!.Equals(dto.Username) ? GameRoom.PlayerBlack! : ""
        };

        GameEvent?.Invoke(new GameRoomEventDto
        {
            GameRoomId = Id,
            GameEventDto = streamDto
        });

        _rematchResponseWithinTimespan = await _rematchCountDownTimer.StartTimer(15);

        if (!_rematchResponseWithinTimespan)
        {
            GameEvent?.Invoke(new GameRoomEventDto
            {
                GameRoomId = Id,
                GameEventDto = new GameEventDto()
                {
                    Event = GameStreamEvents.RematchOfferTimeout,
                    IsWhite = GameRoom.PlayerWhite!.Equals(dto.Username)
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
                IsWhite = GameRoom.PlayerWhite!.Equals(dto.Username)
            }
        });

        return AckTypes.Success;
    }

    public AckTypes RematchOfferResponse(ResponseRematchDto dto)
    {
        if (!_isRematchOffered) return AckTypes.RematchNotOffered;
        if (!dto.Username.Equals(GameRoom.PlayerWhite) && !dto.Username.Equals(GameRoom.PlayerBlack))
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
            _rematchCountDownTimer.StopTimer();
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
        var moveType = (MoveTypes)(dto.MoveType ?? 0);
        var promotionPiece = (PieceTypes?)dto.Promotion;

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
            Creator = GameRoom.Creator,
            UsernameWhite = GameRoom.PlayerWhite!,
            UsernameBlack = GameRoom.PlayerBlack!,
            DurationSeconds = GetInitialTimeControlSeconds,
            IncrementSeconds = GetInitialTimeControlIncrement
        };
    }

    public bool CanUsernameJoin(string username)
    {
        if (GameRoom.GameType is OpponentTypes.Random or OpponentTypes.Ai)
            return true;

        return GameRoom.PlayerWhite!.Equals(username) || GameRoom.PlayerBlack!.Equals(username);
    }
}
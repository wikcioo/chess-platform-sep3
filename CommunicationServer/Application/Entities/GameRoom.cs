using System.Reactive.Linq;
using Application.States;
using Application.Utils;
using Domain.DTOs;
using Domain.Enums;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

namespace Application.Entities;

public class GameRoom
{
    private readonly IGame _game;
    private readonly IGameState _state;
    private readonly List<JoinedGameStreamDto> _gameData = new();
    private readonly ChessTimer _chessTimer;
    private readonly ValueWrapper<bool> _whitePlaying = new(true);
    private bool _firstMovePlayed;
    private bool _gameIsActive = true;
    public event Action<JoinedGameStreamDto> GameJoined = delegate { };
    public string? PlayerWhite { get; set; }
    public string? PlayerBlack { get; set; }

    public GameRoom(GameStateTypes gameType, uint timeControlSeconds, uint timeControlIncrement, string? fen = null)
    {
        _state = gameType switch
        {
            GameStateTypes.Ai => new AiGameState(),
            GameStateTypes.Friend => new FriendGameState(),
            GameStateTypes.Random => new RandomGameState(),
            _ => throw new Exception("Invalid game type.")
        };

        _game = GameFactory.Create();
        _game.NewGame(fen ?? Fen.StartPositionFen);
        
        _gameData.Add(new JoinedGameStreamDto()
        {
            FenString = "initial",
            TimeLeftMs = timeControlSeconds * 1000
        });

        _chessTimer = new ChessTimer(ref _whitePlaying, timeControlSeconds, timeControlIncrement);
        _chessTimer.ThrowEvent += (_, _, dto) =>
        {
            if (dto.GameEndType == (uint) GameEndTypes.TimeIsUp) _gameIsActive = false;
            GameJoined.Invoke(dto);
        };
    }

    public AckTypes MakeMove(MakeMoveDto dto)
    {
        if (!_gameIsActive) return AckTypes.GameHasFinished;

        var move = ParseMove(dto);

        if (!IsValidMove(move))
            return AckTypes.InvalidMove;

        if (!_firstMovePlayed)
        {
            _chessTimer.StartTimers();
            _firstMovePlayed = true;
        }

        if (!_whitePlaying.Value && !dto.Username.Equals(PlayerBlack))
        {
            return AckTypes.IncorrectUser;
        }

        if (_whitePlaying.Value && !dto.Username.Equals(PlayerWhite))
        {
            return AckTypes.IncorrectUser;
        }

        _game.Pos.MakeMove(move, _game.Pos.State);

        _chessTimer.UpdateTimers();

        var responseJoinedGameDto = new JoinedGameStreamDto()
        {
            FenString = _game.Pos.FenNotation,
            TimeLeftMs = !_whitePlaying.Value ? _chessTimer.WhiteRemainingTimeMs : _chessTimer.BlackRemainingTimeMs,
            GameEndType = (uint) _game.GameEndType,
            IsWhite = !_whitePlaying.Value
        };

        _gameData.Add(responseJoinedGameDto);
        GameJoined.Invoke(responseJoinedGameDto);

        return AckTypes.Success;
    }

    private bool IsValidMove(Move move)
    {
        return _game.Pos.GenerateMoves().ToList().Any(valid => move.Equals(valid));
    }

    private Move UciMoveToRudzoftMove(string uci)
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

    public IObservable<JoinedGameStreamDto> GetMovesAsObservable()
    {
        var newMoves = Observable.FromEvent<JoinedGameStreamDto>(
            x => GameJoined += x,
            x => GameJoined -= x);
        return _gameData.ToObservable().Concat(newMoves);
    }
}
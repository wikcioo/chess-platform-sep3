using Domain.DTOs.Stockfish;

namespace StockfishWrapper;

public enum PositionType { Fen, StartPos }

public interface IStockfishUci
{
    void Uci();
    Task<bool> IsReady();
    void SetOption(string name, string? value = null);
    void UciNewGame();
    void Position(string position, PositionType type);
    Task<string?> Go(string? searchMoves = null, bool? ponder = null, int? wTime = null, int? bTime = null,
        int? wInc = null, int? bInc = null, int? movesToGo = null, int? depth = null, int? nodes = null,
        int? mate = null, int? moveTime = null, bool infinite = false);
    Task<string?> Stop();
    void PonderHit();
    Task<bool> SetOptions(StockfishSettingsDto settings);
    void Quit();
}
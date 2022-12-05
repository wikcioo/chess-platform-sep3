using Domain.DTOs.Stockfish;
using Domain.Enums;

namespace StockfishWrapper;

public interface IStockfishUci
{
    void Uci();
    Task<bool> IsReady();
    void SetOption(string name, string? value = null);
    void UciNewGame();
    void Position(string position, PositionType type);
    Task<string?> Go(StockfishGoDto goDto);
    Task<string?> Stop();
    void PonderHit();
    Task<bool> SetOptions(StockfishSettingsDto settings);
    void Quit();
}
using Domain.DTOs.Stockfish;
using Domain.Enums;

namespace StockfishWrapper;

public interface IStockfishUci
{
    void Uci();
    Task<bool> IsReadyAsync();
    void SetOption(string name, string? value = null);
    void UciNewGame();
    void Position(string position, PositionType type);
    Task<string?> GoAsync(StockfishGoDto goDto);
    Task<string?> StopAsync();
    void PonderHit();
    Task<bool> SetOptionsAsync(StockfishSettingsDto settings);
    void Quit();
}
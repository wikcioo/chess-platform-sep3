using Domain.DTOs.Stockfish;

namespace Application.LogicInterfaces;

public interface IStockfishLogic
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
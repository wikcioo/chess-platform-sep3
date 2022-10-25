using Domain.DTOs;

namespace Application.ClientInterfaces;

public interface IStockfishService
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
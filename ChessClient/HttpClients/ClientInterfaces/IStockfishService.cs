using Domain.DTOs;

namespace HttpClients.ClientInterfaces;

public interface IStockfishService
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
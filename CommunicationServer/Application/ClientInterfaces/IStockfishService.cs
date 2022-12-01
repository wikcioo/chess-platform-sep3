using Domain.DTOs;
using Domain.DTOs.StockfishData;

namespace Application.ClientInterfaces;

public interface IStockfishService
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
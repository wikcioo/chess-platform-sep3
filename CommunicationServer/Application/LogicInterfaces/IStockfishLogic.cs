using System.Threading.Tasks;
using Domain.DTOs;
using Domain.DTOs.StockfishData;

namespace Application.LogicInterfaces;

public interface IStockfishLogic
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.LogicInterfaces;

public interface IStockfishLogic
{
    Task<bool> GetStockfishIsReadyAsync();
    Task<string> GetBestMoveAsync(StockfishBestMoveDto dto);
}
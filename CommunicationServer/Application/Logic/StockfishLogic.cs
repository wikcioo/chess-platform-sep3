using System.Threading.Tasks;
using Application.ClientInterfaces;
using Application.LogicInterfaces;
using Domain.DTOs;

namespace Application.Logic;

public class StockfishLogic : IStockfishLogic
{
    private readonly IStockfishService _stockfishService;

    public StockfishLogic(IStockfishService stockfishService)
    {
        _stockfishService = stockfishService;
    }

    public async Task<bool> GetStockfishIsReadyAsync()
    {
        return await _stockfishService.GetStockfishIsReadyAsync();
    }

    public async Task<string> GetBestMoveAsync(StockfishBestMoveDto dto)
    {
        return await _stockfishService.GetBestMoveAsync(dto);
    }
}
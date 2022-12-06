using Domain.DTOs.Stockfish;
using Rudzoft.ChessLib.Fen;

namespace Application.LogicInterfaces;

public interface IStockfishLogic
{
    Task<IFenData> GetBestMoveAsync(StockfishBestMoveDto dto);
    Task<bool> IsReadyAsync();
}
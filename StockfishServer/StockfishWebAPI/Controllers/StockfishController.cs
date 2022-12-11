using Application.LogicInterfaces;
using Domain.DTOs.Stockfish;
using Grpc.Core;
using StockfishGrpc;

namespace StockfishWebAPI.Controllers;

public class StockfishController : StockfishService.StockfishServiceBase
{
    private readonly IStockfishLogic _stockfishLogic;

    public StockfishController(IStockfishLogic stockfishLogic)
    {
        _stockfishLogic = stockfishLogic;
    }

    public override async Task<IsReady> GetStockfishReady(Empty request, ServerCallContext context)
    {
        var isReady = await _stockfishLogic.IsReadyAsync();
        return new IsReady
        {
            Ready = isReady
        };
    }

    public override async Task<BestMove> GetBestMove(RequestBestMove request, ServerCallContext context)
    {
        try
        {
            var fen = await _stockfishLogic.GetBestMoveAsync(new StockfishBestMoveDto(request.Fen,
                request.StockfishPlayer));

            return new BestMove
            {
                Fen = fen.Fen.ToString()
            };
        }
        catch (InvalidOperationException e)
        {
            throw new RpcException(Status.DefaultCancelled, e.Message);
        }
        catch (ArgumentException e)
        {
            throw new RpcException(Status.DefaultCancelled, e.Message);
        }
    }
}
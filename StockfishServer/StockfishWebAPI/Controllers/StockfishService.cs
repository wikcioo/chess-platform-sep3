using Grpc.Core;
using StockfishWrapper;

namespace StockfishWebAPI.Controllers;

public class StockfishService : Stockfish.StockfishBase
{
    private readonly IStockfishUci _stockfish;

    public StockfishService(IStockfishUci stockfish)
    {
        _stockfish = stockfish;
    }


    public override async Task<IsReady> GetStockfishReady(Empty request, ServerCallContext context)
    {
        var isReady = await _stockfish.IsReady();
        return new IsReady
        {
            Ready = isReady
        };
    }


    public override async Task<BestMove> GetBestMove(RequestBestMove request, ServerCallContext context)
    {
        // TODO(Wiktor): Add validation for fen and depth value
        _stockfish.UciNewGame();
        _stockfish.Position(request.Fen, PositionType.Fen);

        var levels = StockfishLevels.LevelOf[request.StockfishPlayer];

        var bestMove = await _stockfish.Go(depth: (int) levels.Depth, moveTime: (int) levels.Time);
        return new BestMove
        {
            Fen = bestMove
        };
    }

    private static bool ValidateStockfishSettings(StockfishSettingsDto settings)
    {
        return settings.Threads is >= 1 and <= 512
               && settings.Hash is >= 1 and <= 33554432
               && settings.MultiPv is >= 1 and <= 500
               && settings.SkillLevel is >= 0 and <= 20;
    }
}
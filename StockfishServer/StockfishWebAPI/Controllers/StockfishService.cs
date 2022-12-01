using Domain.DTOs.Stockfish;
using Domain.Models;
using Grpc.Core;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Validation;
using StockfishGrpc;
using StockfishWrapper;

namespace StockfishWebAPI.Controllers;

public class StockfishService : StockfishGrpc.StockfishService.StockfishServiceBase
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
        try
        {
            Fen.Validate(request.Fen);
        }
        catch (InvalidFen e)
        {
            Console.WriteLine(e);
            throw new RpcException(Status.DefaultCancelled, "Invalid fen!");
        }

        _stockfish.UciNewGame();
        _stockfish.Position(request.Fen, PositionType.Fen);

        var levels = StockfishLevels.LevelOf[request.StockfishPlayer];

        if (!await _stockfish.SetOptions(new StockfishSettingsDto() { SkillLevel = (int)levels.Skill }))
        {
            Console.WriteLine("[StockfishService]: Failed to set options!");
        }
        
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
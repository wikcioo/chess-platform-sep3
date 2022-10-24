using Microsoft.AspNetCore.Mvc;
using StockfishWrapper;

namespace StockfishWebAPI.Controllers;

[ApiController]
[Route("api")]
public class StockfishController : ControllerBase
{
    private readonly IStockfishUci _stockfish;

    public StockfishController(IStockfishUci stockfish)
    {
        _stockfish = stockfish;
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetBestMoveAsync(string fen, int depth)
    {
        try
        {
            _stockfish.UciNewGame();
            _stockfish.Position(fen, PositionType.Fen);
            var bestMove = await _stockfish.Go(depth: depth);
            return Ok(bestMove);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}
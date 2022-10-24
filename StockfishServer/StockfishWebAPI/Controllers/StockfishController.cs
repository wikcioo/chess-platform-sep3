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
    
    [HttpGet("isReady")]
    public async Task<ActionResult<bool>> GetStockfishReadyAsync()
    {
        try
        {
            var isReady = await _stockfish.IsReady();
            return Ok(isReady);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("bestMove")]
    public async Task<ActionResult<string>> GetBestMoveAsync(string fen, int depth)
    {
        try
        {
            // TODO(Wiktor): Add validation for fen and depth value
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

    [HttpPut]
    public async Task<ActionResult> SetOptionsAsync(StockfishSettingsDto settings)
    {
        try
        {
            if (ValidateStockfishSettings(settings) == false)
                return BadRequest();
            
            var wasSuccessful = await _stockfish.SetOptions(settings);
            if (wasSuccessful)
                return Ok();
            
            return BadRequest();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    private static bool ValidateStockfishSettings(StockfishSettingsDto settings)
    {
        return settings.Threads is >= 1 and <= 512 
               && settings.Hash is >= 1 and <= 33554432 
               && settings.MultiPv is >= 1 and <= 500 
               && settings.SkillLevel is >= 0 and <= 20;
    }
}
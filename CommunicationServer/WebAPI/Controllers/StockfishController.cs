using Application.LogicInterfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StockfishController : ControllerBase
{
    private readonly IStockfishLogic _stockfishLogic;

    public StockfishController(IStockfishLogic stockfishLogic)
    {
        _stockfishLogic = stockfishLogic;
    }

    [HttpGet("/isReady")]
    public async Task<ActionResult<bool>> GetStockfishIsReadyAsync()
    {
        try
        {
            var status = await _stockfishLogic.GetStockfishIsReadyAsync();
            return Ok(status);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/bestMove")]
    public async Task<ActionResult<string>> GetBestMoveAsync([FromBody] StockfishBestMoveDto dto)
    {
        try
        {
            var result = await _stockfishLogic.GetBestMoveAsync(dto);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}
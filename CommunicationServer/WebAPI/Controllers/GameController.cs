using Application.ClientInterfaces;
using Application.Hubs;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly GroupHandler _groupHandler;
    private readonly IGameLogic _gameLogic;
    private readonly IStockfishService _stockfishService;

    public GameController(IGameLogic gameLogic,IStockfishService stockfishService, GroupHandler groupHandler)
    {
        _gameLogic = gameLogic;
        _stockfishService = stockfishService;
        _groupHandler = groupHandler;
    }

    [HttpPost("/startGame")]
    public async Task<ActionResult<ResponseGameDto>> StartGame([FromBody] RequestGameDto request)
    {
        try
        {
            request.Username = User.Identity.Name;
            if (request.OpponentType == OpponentTypes.Ai)
            {
                var aiIsReady = await _stockfishService.GetStockfishIsReadyAsync();
                if (!aiIsReady)
                {
                    return StatusCode(500, "Cannot create game. Ai is not ready."); 
                }
                
            }

            var response = await _gameLogic.StartGame(request);
            return Ok(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/joinGame")]
    public ActionResult<AckTypes> JoinGame([FromBody] RequestJoinGameDto dto)
    {
        try
        {
            dto.Username = User.Identity.Name;
            var ack = _gameLogic.JoinGame(dto);
            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }


    [HttpPost("/gameState")]
    public ActionResult<CurrentGameStateDto> GetCurrentGameState([FromBody] ulong gameRoomId)
    {
        try
        {
            var result = _gameLogic.GetCurrentGameState(gameRoomId);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/resign")]
    public async Task<ActionResult<AckTypes>> Resign([FromBody] RequestResignDto request)
    {
        try
        {
            AckTypes ack = await _gameLogic.Resign(new RequestResignDto()
            {
                GameRoom = request.GameRoom,
                Username = User.Identity.Name
            });


            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/makeMove")]
    public async Task<ActionResult<AckTypes>> MakeMove([FromBody] MakeMoveDto dto)
    {
        try
        {
            dto.Username = User.Identity.Name;
            var ack = await _gameLogic.MakeMove(dto);


            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/offerDraw")]
    public async Task<ActionResult<AckTypes>> OfferDraw([FromBody] RequestDrawDto request)
    {
        try
        {
            var ack = await _gameLogic.OfferDraw(new RequestDrawDto()
            {
                GameRoom = request.GameRoom,
                Username = User.Identity.Name
            });

            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/drawResponse")]
    public async Task<ActionResult<AckTypes>> DrawOfferResponse([FromBody] ResponseDrawDto request)
    {
        try
        {
            var ack = await _gameLogic.DrawOfferResponse(request);
            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/spectateable")]
    public ActionResult<IEnumerable<SpectateableGameRoomDataDto>> GetSpectateableGames()
    {
        try
        {
            var result = _gameLogic.GetSpectateableGameRoomData();
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/joinable")]
    public ActionResult<IEnumerable<JoinableGameRoomDataDto>> GetJoinableGames()
    {
        try
        {
            var result = _gameLogic.GetJoinableGameRoomData(User.Identity.Name);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}
using Application.ClientInterfaces;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
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

    public GameController(IGameLogic gameLogic, IStockfishService stockfishService, GroupHandler groupHandler)
    {
        _gameLogic = gameLogic;
        _stockfishService = stockfishService;
        _groupHandler = groupHandler;
    }

    [HttpPost("/games")]
    public async Task<ActionResult<ResponseGameDto>> StartGame([FromBody] RequestGameDto request)
    {
        try
        {
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            }

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

    [HttpPost("/games/{id}/users")]
    public ActionResult<AckTypes> JoinGame(ulong id)
    {
        if (User.Identity?.Name == null)
        {
            return StatusCode(401, "Identity not found.");
        }

        try
        {
            var dto = new RequestJoinGameDto()
            {
                GameRoom = id,
                Username = User.Identity.Name
            };
            var ack = _gameLogic.JoinGame(dto);
            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/games/{id}/spectators")]
    public ActionResult<AckTypes> SpectateGame(ulong id)
    {
        if (User.Identity?.Name == null)
        {
            return StatusCode(401, "Identity not found.");
        }

        try
        {
            var dto = new RequestJoinGameDto()
        {
            GameRoom = id,
            Username = User.Identity.Name
        }; 
            var ack = _gameLogic.SpectateGame(dto); 
            return Ok(ack);
    }
        catch (Exception e)
    {
        Console.WriteLine(e);
        return StatusCode(500, e.Message);
    }
}


[HttpGet("/games/{id}")]
    public ActionResult<CurrentGameStateDto> GetCurrentGameState(ulong id)
    {
        try
        {
            var result = _gameLogic.GetCurrentGameState(id);
            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/resignation")]
    public async Task<ActionResult<AckTypes>> Resign([FromBody] RequestResignDto request)
    {
        try
        {
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            } 
            
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

    [HttpPost("/moves")]
    public async Task<ActionResult<AckTypes>> MakeMove([FromBody] MakeMoveDto dto)
    {
        try
        {
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            } 
            
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

    [HttpPost("/draw-offers")]
    public async Task<ActionResult<AckTypes>> OfferDraw([FromBody] RequestDrawDto request)
    {
        try
        {
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            }
            
            request.Username = User.Identity.Name;
            var ack = await _gameLogic.OfferDraw(request);

            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("/draw-responses")]
    public async Task<ActionResult<AckTypes>> DrawOfferResponse([FromBody] ResponseDrawDto request)
    {
        try
        {
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            }
            
            request.Username = User.Identity.Name;
            var ack = await _gameLogic.DrawOfferResponse(request);
            return Ok(ack);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet("/rooms")]
    public ActionResult<IEnumerable<GameRoomDto>> GetRooms([FromQuery] bool spectateable, [FromQuery] bool joinable)
    {
        try
        {
            
            if (User.Identity?.Name == null)
            {
                return StatusCode(401, "Identity not found.");
            } 
            
            var rooms = _gameLogic.GetGameRooms(new GameRoomSearchParameters()
            {
                RequesterName = User.Identity.Name,
                Joinable = joinable,
                Spectateable = spectateable
            });

            return Ok(rooms);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}
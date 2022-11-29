using System.Security.Claims;
using Application.Hubs;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameLogic _gameLogic;
    private readonly IHubContext<GameHub> _hubContext;

    public GameController(IGameLogic gameLogic, IHubContext<GameHub> hubContext)
    {
        _gameLogic = gameLogic;
        _hubContext = hubContext;
    }

    [HttpPost("/startGame")]
    public async Task<ResponseGameDto> StartGame([FromBody] RequestGameDto request)
    {
        request.Username = User.Identity.Name;
        var response = await _gameLogic.StartGame(request);

        return response;
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
    public async Task<AckTypes> Resign([FromBody] RequestResignDto request)
    {
        // var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        // if (claim == null)
        // {
        //     return new AckTypes()
        //     {
        //         Status = (uint) AckTypes.NotUserTurn
        //     };
        // }

        AckTypes ack = await _gameLogic.Resign(new RequestResignDto()
        {
            GameRoom = request.GameRoom,
            Username = User.Identity.Name
        });
        return ack;
    }

    [HttpPost("/makeMove")]
    public async Task<AckTypes> MakeMove([FromBody] MakeMoveDto dto)
    {
        dto.Username = User.Identity.Name;
        var ack = await _gameLogic.MakeMove(dto);
        return ack;
    }

    [HttpPost("/offerDraw")]
    public async Task<AckTypes> OfferDraw([FromBody] RequestDrawDto request)
    {
        // var claim = context.GetHttpContext().User.Claims.FirstOrDefault(claim => claim.Type.Equals(ClaimTypes.Name));
        // if (claim == null)
        // {
        //     return AckTypes.NotUserTurn;
        // }

        var ack = await _gameLogic.OfferDraw(new RequestDrawDto()
        {
            GameRoom = request.GameRoom,
            Username = User.Identity.Name
        });
        return ack;
    }

    [HttpPost("/drawResponse")]
    public async Task<AckTypes> DrawOfferResponse([FromBody] ResponseDrawDto request)
    {
        AckTypes ack = await _gameLogic.DrawOfferResponse(request);
        return ack;
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
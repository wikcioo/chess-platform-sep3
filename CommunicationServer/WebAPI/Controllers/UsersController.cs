using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserLogic _userLogic;

    public UsersController(IUserLogic userLogic)
    {
        _userLogic = userLogic;
    }

    [HttpPost("/login")]
    public async Task<ActionResult> LoginAsync([FromBody] UserLoginDto dto)
    {
        try
        {
            await _userLogic.LoginAsync(dto);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateAsync([FromBody] User user)
    {
        try
        {
            await _userLogic.CreateAsync(user);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
}
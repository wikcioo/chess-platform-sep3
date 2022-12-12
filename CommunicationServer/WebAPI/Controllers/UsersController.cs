using Application.LogicInterfaces;
using Domain.DTOs.User;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<bool>> LoginAsync([FromBody] UserLoginDto dto)
    {
        try
        {
            var isCorrect = await _userLogic.LoginAsync(dto);
            return Ok(isCorrect);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<User>> CreateAsync(User user)
    {
        try
        {
            var created = await _userLogic.CreateAsync(user);
            return Ok(created);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet("/users")]
    [Authorize]
    public async Task<ActionResult<UserSearchResultDto>> GetAsync([FromQuery] string? username)
    {
        try
        {
            var searchResult = await _userLogic.GetInsensitiveAsync(new UserSearchParamsDto(username));
            return Ok(searchResult);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
}
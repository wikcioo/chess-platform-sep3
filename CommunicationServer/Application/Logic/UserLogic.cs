using Application.DaoInterfaces;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.Models;

namespace Application.Logic;

public class UserLogic : IUserLogic
{
    private readonly IUserService _userService;

    public UserLogic(IUserService userService)
    {
        _userService = userService;
    }

    public async Task LoginAsync(UserLoginDto dto)
    {
        var answer = await _userService.LoginAsync(dto);
        Console.WriteLine(answer ? "Correct password" : "Incorrect password");
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _userService.CreateAsync(user);
    }
}
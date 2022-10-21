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

    public async Task<bool> LoginAsync(UserLoginDto dto)
    {
        return await _userService.LoginAsync(dto);
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _userService.CreateAsync(user);
    }
}
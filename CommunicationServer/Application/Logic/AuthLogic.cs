using Application.DaoInterfaces;
using Domain.DTOs;
using Domain.Models;
using WebAPI.Services;

namespace Application.Logic;

public class AuthLogic : IAuthLogic
{
    private readonly IUserService _userService;

    public AuthLogic(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<User> ValidateUser(string email, string password)
    {
        var existingUser = await _userService.LoginAsync(new UserLoginDto {Email = email, Password = password});

        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        if (!existingUser.Password.Equals(password))
        {
            throw new Exception("Password mismatch");
        }

        return existingUser;
    }
}
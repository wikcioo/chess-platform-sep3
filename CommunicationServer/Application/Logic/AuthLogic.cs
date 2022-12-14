using Application.ClientInterfaces;
using Application.LogicInterfaces;
using Domain.DTOs.User;
using Domain.Models;

namespace Application.Logic;

public class AuthLogic : IAuthLogic
{
    private readonly IUserService _userService;

    public AuthLogic(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<User> LoginAsync(string email, string password)
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
using System.ComponentModel.DataAnnotations;
using Application.ClientInterfaces;
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

    public async Task<User> LoginAsync(UserLoginDto dto)
    {
        return await _userService.LoginAsync(dto);
    }

    public async Task<User> CreateAsync(User user)
    {
        ValidateNewUser(user);
        return await _userService.CreateAsync(user);
    }

    private static void ValidateNewUser(User user)
    {
        if (user.Username.Length > 16) throw new ArgumentException("Username has to be less than 16 characters!");
        if (!new EmailAddressAttribute().IsValid(user.Email)) throw new ArgumentException("Invalid email address!");
        if (user.Password.Length < 3) throw new ArgumentException("Password has to be at least 3 characters!");
    }

    public async Task<IEnumerable<UserSearchResultDto>> GetInsensitiveAsync(UserSearchParamsDto paramsDto)
    {
        var users = await _userService.GetAsync(paramsDto);
        
        return users.Select(u => new UserSearchResultDto(u.Username)).ToList();
    }
}
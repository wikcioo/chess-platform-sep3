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
        return await _userService.CreateAsync(user);
    }

    public async Task<IEnumerable<UserSearchResultDto>> GetInsensitiveAsync(UserSearchParamsDto paramsDto)
    {
        var users = await _userService.GetAsync(paramsDto);
        
        return users.Select(u => new UserSearchResultDto(u.Username)).ToList();
    }
}
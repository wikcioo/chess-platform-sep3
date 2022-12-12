using Domain.DTOs.User;
using Domain.Models;

namespace Application.LogicInterfaces;

public interface IUserLogic
{
    Task<User> LoginAsync(UserLoginDto dto);
    Task<User> CreateAsync(User user);

    Task<IEnumerable<UserSearchResultDto>> GetInsensitiveAsync(UserSearchParamsDto paramsDto);
}
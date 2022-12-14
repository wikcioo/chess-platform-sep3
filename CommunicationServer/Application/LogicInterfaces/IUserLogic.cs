using Domain.DTOs.User;
using Domain.Models;

namespace Application.LogicInterfaces;

public interface IUserLogic
{
    Task<User> CreateAsync(User user);

    Task<IEnumerable<UserSearchResultDto>> GetInsensitiveAsync(UserSearchParamsDto paramsDto);
}
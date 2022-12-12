using Domain.DTOs.User;
using Domain.Models;

namespace HttpClients.ClientInterfaces;

public interface IUserService
{
    Task<User> CreateAsync(User user);
    Task<IEnumerable<UserSearchResultDto>> GetAsync(UserSearchParamsDto paramsDto);
}
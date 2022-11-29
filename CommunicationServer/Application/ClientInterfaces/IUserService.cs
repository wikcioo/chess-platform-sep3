using Domain.DTOs;
using Domain.Models;

namespace Application.DaoInterfaces;

public interface IUserService
{
    Task<User> LoginAsync(UserLoginDto dto);
    Task<User> CreateAsync(User user);
    Task<IEnumerable<User>> GetAsync(UserSearchParamsDto paramsDto);

}
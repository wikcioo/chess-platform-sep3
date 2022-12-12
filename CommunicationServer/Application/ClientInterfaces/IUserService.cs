using Domain.DTOs.User;
using Domain.Models;

namespace Application.ClientInterfaces;

public interface IUserService
{
    Task<User> LoginAsync(UserLoginDto dto);
    Task<User> CreateAsync(User user);
    Task<IEnumerable<User>> GetAsync(UserSearchParamsDto paramsDto);
    Task<User?> GetByUsernameAsync(string username);

}
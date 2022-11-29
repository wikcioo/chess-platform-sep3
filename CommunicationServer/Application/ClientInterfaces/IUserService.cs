using Domain.DTOs;
using Domain.Models;

namespace Application.ClientInterfaces;

public interface IUserService
{
    Task<User> LoginAsync(UserLoginDto dto);
    Task<User> CreateAsync(User user);
    
}
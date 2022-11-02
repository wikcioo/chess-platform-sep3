using Domain.DTOs;
using Domain.Models;

namespace Application.LogicInterfaces;

public interface IUserLogic
{
    Task<User> LoginAsync(UserLoginDto dto);
    Task<User> CreateAsync(User user);
}
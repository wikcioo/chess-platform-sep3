using Domain.DTOs;
using Domain.Models;

namespace Application.DaoInterfaces;

public interface IUserService
{
    Task<bool> LoginAsync(UserLoginDto dto);
}
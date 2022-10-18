using Domain.DTOs;
using Domain.Models;

namespace Application.DaoInterfaces;

public interface IUserService
{
    Task LoginAsync(UserLoginDto dto);
}
using Domain.DTOs;

namespace Application.LogicInterfaces;

public interface IUserLogic
{
    Task LoginAsync(UserLoginDto dto);
}
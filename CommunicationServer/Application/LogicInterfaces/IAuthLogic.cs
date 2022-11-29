using Domain.Models;

namespace Application.LogicInterfaces;

public interface IAuthLogic
{
    Task<User> ValidateUser(string username, string password);
}
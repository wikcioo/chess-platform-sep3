using Domain.Models;

namespace Application.LogicInterfaces;

public interface IAuthLogic
{
    Task<User> LoginAsync(string email, string password);
}
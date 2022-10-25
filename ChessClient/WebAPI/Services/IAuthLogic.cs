using Domain.Models;

namespace WebAPI.Services;

public interface IAuthLogic
{
    Task<User> GetUser(string username, string password);
    Task CreateUser(User user);
}
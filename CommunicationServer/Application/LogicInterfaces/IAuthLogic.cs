using Domain.Models;

namespace WebAPI.Services;

public interface IAuthLogic
{
    Task<User> ValidateUser(string username, string password);
}
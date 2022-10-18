
using Application.DaoInterfaces;
using Domain.DTOs;

namespace DatabaseClient.Implementations;

public class UserHttpClient : IUserService
{
    public async Task LoginAsync(UserLoginDto dto)
    {
        throw new NotImplementedException();
    }
}
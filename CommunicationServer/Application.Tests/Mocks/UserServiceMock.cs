namespace Application.Tests.Mocks;

using ClientInterfaces;
using Domain.DTOs.User;
using Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class UserServiceMock : IUserService
{
    private List<User> _context = new()
    {
        new("username", "email", "password", "user"),
        new("Bob", "Bob@email.eu", "password", "user"),
        new("Jim", "Jim@email.eu", "password", "user"),
        new("Alice", "Alice@email.eu", "password", "user"),
        new("Agatha", "Agatha@email.eu", "password", "admin"),
    };
    public Task<User> LoginAsync(UserLoginDto dto)
    {
        return Task.FromResult(_context.Find(u => dto.Email == u.Email));
    }

    public Task<User> CreateAsync(User user)
    {
        _context.Add(user);

        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAsync(UserSearchParamsDto paramsDto)
    {
        return Task.FromResult<IEnumerable<User>>(_context.FindAll(u => u.Username.Contains(paramsDto.Username)));
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return Task.FromResult(_context.FirstOrDefault(u => u.Username.Equals(username), null));
    }
}
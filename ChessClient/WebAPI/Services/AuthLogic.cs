using System.ComponentModel.DataAnnotations;
using Domain.Models;

namespace WebAPI.Services;

public class AuthLogic:IAuthLogic
{
    private readonly IList<User> users = new List<User>
    {
        new User
        {
            Email = "wiktor@via.dk",
            Password = "chess",
            Role = "admin",
            Username = "wikichoo",

        },
        new User
        {

            Email = "morek@via.dk",
            Password = "game",
            Role = "user",
            Username = "morek",
        },
        
        new User
        {

            Email = "aivaras@via.dk",
            Password = "chessgame",
            Role = "user",
            Username = "letters",
        }
    };

    public Task<User> ValidateUser(string username, string password)
    {
        User? existingUser = users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        if (existingUser == null)
        {
            throw new Exception("User not found");
        }

        if (!existingUser.Password.Equals(password))
        {
            throw new Exception("Password mismatch");
        }

        return Task.FromResult(existingUser);
    }

    public Task CreateUser(User user)
    {

        if (string.IsNullOrEmpty(user.Username))
        {
            throw new ValidationException("Username cannot be null");
        }

        if (string.IsNullOrEmpty(user.Password))
        {
            throw new ValidationException("Password cannot be null");
        }
        // Do more user info validation here
        
        // save to persistence instead of list
        
        users.Add(user);
        
        return Task.CompletedTask;
    }

    public Task<User> GetUser(string username, string password)
    {
        throw new NotImplementedException();
    }
    
}
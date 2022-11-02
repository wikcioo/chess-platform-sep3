namespace Domain.Models;

public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }

    public User()
    {
    }

    public User(string username, string email, string password, string role)
    {
        Username = username;
        Email = email;
        Password = password;
        Role = role;
    }
}
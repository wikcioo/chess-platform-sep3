namespace Domain.DTOs;

public class UserLoginDto
{
    public string Email { get; }
    public string Password { get; }

    public UserLoginDto(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
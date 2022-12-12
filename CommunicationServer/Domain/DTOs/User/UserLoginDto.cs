namespace Domain.DTOs.User;

public class UserLoginDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
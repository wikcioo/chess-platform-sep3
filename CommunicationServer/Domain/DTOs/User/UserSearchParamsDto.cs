namespace Domain.DTOs.User;

public class UserSearchParamsDto
{
    public string? Username { get; set; }

    public UserSearchParamsDto(string? username)
    {
        Username = username;
    }
}
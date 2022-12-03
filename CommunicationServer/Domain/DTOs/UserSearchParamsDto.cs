namespace Domain.DTOs;

public class UserSearchParamsDto
{
    public string? Username { get; set; }

    public UserSearchParamsDto(string? username)
    {
        Username = username;
    }
}
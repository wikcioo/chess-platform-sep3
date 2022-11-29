namespace Domain.DTOs;

public class UserSearchResultDto
{
    public string Username { get; set; }

    public UserSearchResultDto(string username)
    {
        Username = username;
    }
}
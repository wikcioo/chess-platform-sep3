namespace Domain.DTOs.User;

public class UserSearchResultDto
{
    public string Username { get; set; }

    public UserSearchResultDto(string username)
    {
        Username = username;
    }
}
namespace Domain.DTOs.Chat;

public class GameRoomSearchParameters
{
    public string RequesterName { get; set; }
    public bool Joinable { get; set; }
    public bool Spectateable { get; set; }
}
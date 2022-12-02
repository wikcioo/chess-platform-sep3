namespace Domain.DTOs;

public class GameRoomSearchParameters
{
    public string RequesterName { get; set; } = string.Empty;
    public bool Joinable { get; set; }
    public bool Spectateable { get; set; }
}
namespace Domain.DTOs;

public class RequestGameDto
{
    public string Username {get;set;}
    public string GameType {get;set;}
    public string? Opponent {get;set;}
    public uint Seconds {get;set;}
    public uint Increment {get;set;}
    public bool? IsWhite {get;set;}
}
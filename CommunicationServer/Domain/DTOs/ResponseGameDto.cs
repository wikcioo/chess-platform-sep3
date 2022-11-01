namespace Domain.DTOs;

public class ResponseGameDto
{
    public bool Success {get;set;}
    public ulong GameRoom {get;set;}
    public string Opponent {get;set;}
    public string Fen{get;set;}
    public bool IsWhite {get;set;}
}
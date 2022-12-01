namespace Domain.DTOs;

public class ResponseGameDto
{
    public bool Success {get;set;}
    public ulong GameRoom {get;set;}
    public string Opponent {get;set;} = String.Empty;
    public string Fen{get;set;} = String.Empty;
    public bool IsWhite {get;set;}
    public string ErrorMessage { get; set; } = string.Empty; 
}
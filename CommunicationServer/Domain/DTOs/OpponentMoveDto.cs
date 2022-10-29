namespace Domain.DTOs;

public class OpponentMoveDto
{
    public string Fen {get;set;}
    public string? GameEndType {get;set;}
    public ulong MoveMadeTime {get;set;}
    public ulong TimeLeft {get;set;}
}
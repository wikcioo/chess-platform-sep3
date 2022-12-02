using Domain.Enums;

namespace Domain.DTOs;

public class RequestGameDto
{
    public string Username { get; set; } = string.Empty;
    public OpponentTypes OpponentType { get; set; }
    public string? OpponentName { get; set; }
    public uint Seconds { get; set; }
    public uint Increment { get; set; }
    public GameSides Side { get; set; }
    public bool IsVisible { get; set; } = true;
}
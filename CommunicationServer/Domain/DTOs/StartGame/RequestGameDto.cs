using Domain.Enums;

namespace Domain.DTOs.StartGame;

public class RequestGameDto
{
    public string Username { get; set; } = string.Empty;
    public OpponentTypes OpponentType { get; set; }
    public string? OpponentName { get; set; }
    public uint DurationSeconds { get; set; }
    public uint IncrementSeconds { get; set; }
    public GameSides Side { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsRematch { get; set; }
}
using Domain.Enums;

namespace Domain.DTOs;

public class RequestGameDto
{
    public string Username { get; set; }
    public OpponentTypes OpponentType { get; set; }
    public string? OpponentName { get; set; }
    public uint Seconds { get; set; }
    public uint Increment { get; set; }
    public GameSides Side { get; set; }
    public bool IsVisible { get; set; } = true;

    public override string ToString()
    {
        return $"{nameof(Username)}: {Username}, {nameof(OpponentType)}: {OpponentType}, {nameof(OpponentName)}: {OpponentName}, {nameof(Seconds)}: {Seconds}, {nameof(Increment)}: {Increment}, {nameof(Side)}: {Side}, {nameof(IsVisible)}: {IsVisible}";
    }
}
namespace Domain.DTOs;

public class JoinedGameStreamDto
{
    public string FenString { get; set; } = string.Empty;
    public uint GameEndType { get; set; }
    public double TimeLeftMs { get; set; }
    public bool IsWhite { get; set; }
    public string Event { get; set; } = string.Empty;
}
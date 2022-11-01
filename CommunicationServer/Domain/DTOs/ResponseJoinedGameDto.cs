namespace Domain.DTOs;

public class ResponseJoinedGameDto
{
    public string FenString { get; set; }
    public uint GameEndType { get; set; }
    public double TimeLeftMs { get; set; }
    public bool IsWhite { get; set; }
}
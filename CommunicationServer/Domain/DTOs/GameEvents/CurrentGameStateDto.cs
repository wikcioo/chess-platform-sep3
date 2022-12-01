namespace Domain.DTOs.GameEvents;

public class CurrentGameStateDto
{
    public string FenString { get; set; } = string.Empty;
    public double WhiteTimeLeftMs { get; set; }
    public double BlackTimeLeftMs { get; set; }
    public string UsernameWhite { get; set; } = string.Empty;
    public string UsernameBlack { get; set; } = string.Empty;
}
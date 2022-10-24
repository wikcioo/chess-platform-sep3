namespace StockfishWrapper;

public struct StockfishSettingsDto
{
    public int Threads { get; set; }
    public int Hash { get; set; }
    public bool Ponder { get; set; }
    public int MultiPv { get; set; }
    public int SkillLevel { get; set; }
}
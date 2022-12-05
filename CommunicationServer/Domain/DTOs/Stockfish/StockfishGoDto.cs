namespace Domain.DTOs.Stockfish;

public class StockfishGoDto
{
    public string? SearchMoves { get; set; } = null;
    public bool? Ponder { get; set; } = null;
    public int? WTime { get; set; } = null;
    public int? BTime { get; set; } = null;
    public int? WInc { get; set; } = null;
    public int? BInc { get; set; } = null;
    public int? MovesToGo { get; set; } = null;
    public int? Depth { get; set; } = null;
    public int? Nodes { get; set; } = null;
    public int? Mate { get; set; } = null;
    public int? MoveTime { get; set; } = null;
    public bool Infinite { get; set; } = false;
}
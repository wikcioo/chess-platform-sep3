namespace StockfishWrapper;

public class StockfishBestMoveDto
{
    public string Fen { get; set; }
    public int Depth { get; set; }

    public StockfishBestMoveDto()
    {
    }

    public StockfishBestMoveDto(string fen, int depth)
    {
        Fen = fen;
        Depth = depth;
    }
}
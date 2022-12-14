namespace Domain.DTOs.Stockfish;

public class StockfishBestMoveDto
{
    public string Fen { get; set; }
    public string StockfishPlayer { get; set; }

    public StockfishBestMoveDto(string fen, string stockfishPlayer)
    {
        Fen = fen;
        StockfishPlayer = stockfishPlayer;
    }
}
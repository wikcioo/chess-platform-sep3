using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace StockfishWrapper;

public class StockfishLevels
{
    private StockfishLevels(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static readonly IReadOnlyDictionary<string, StockfishPlayer> LevelOf =
        new Dictionary<string, StockfishPlayer>
        {
            { StockfishAi1.Value, new StockfishPlayer(3, 1, 50) },
            { StockfishAi2.Value, new StockfishPlayer(6, 2, 100) },
            { StockfishAi3.Value, new StockfishPlayer(9, 3, 150) },
            { StockfishAi4.Value, new StockfishPlayer(11, 4, 200) },
            { StockfishAi5.Value, new StockfishPlayer(14, 6, 250) },
            { StockfishAi6.Value, new StockfishPlayer(17, 8, 300) },
            { StockfishAi7.Value, new StockfishPlayer(20, 10, 350) },
            { StockfishAi8.Value, new StockfishPlayer(20, 12, 400) },
        };

    public static StockfishLevels StockfishAi1 => new StockfishLevels("StockfishAi1");
    public static StockfishLevels StockfishAi2 => new StockfishLevels("StockfishAi2");
    public static StockfishLevels StockfishAi3 => new StockfishLevels("StockfishAi3");
    public static StockfishLevels StockfishAi4 => new StockfishLevels("StockfishAi4");
    public static StockfishLevels StockfishAi5 => new StockfishLevels("StockfishAi5");
    public static StockfishLevels StockfishAi6 => new StockfishLevels("StockfishAi6");
    public static StockfishLevels StockfishAi7 => new StockfishLevels("StockfishAi7");
    public static StockfishLevels StockfishAi8 => new StockfishLevels("StockfishAi8");


    public static bool IsAi(string? playerName) =>
        playerName != null && Regex.IsMatch(playerName, @"(StockfishAi)[0-9]{1}");
    
    
    
}
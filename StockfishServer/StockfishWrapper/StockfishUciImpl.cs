using System.Diagnostics;
using System.Text;

namespace StockfishWrapper;

public class StockfishUciImpl : IStockfishUci
{
    private readonly Process _process;

    public StockfishUciImpl(string? path = null)
    {
        _process = new Process();
        _process.StartInfo.FileName = string.IsNullOrEmpty(path) ? GetStockfishBinaryFilePath() : path;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.StartInfo.RedirectStandardInput = true;
        _process.StartInfo.RedirectStandardError = true;
        _process.StartInfo.UseShellExecute = false;

        _process.Start();
    }

    public void Uci()
    {
        RunCmd("uci");
    }

    public async Task<bool> IsReady()
    {
        RunCmd("isready");

        var result = await WaitForStringOrToken("readyok");
        return !string.IsNullOrEmpty(result) && result == "readyok";
    }

    public void SetOption(string name, string? value = null)
    {
        var command = new StringBuilder($"setoption name {name}");
        if (value != null)
            command.Append($" value {value}");

        RunCmd(command.ToString());
    }

    public void UciNewGame()
    {
        RunCmd("ucinewgame");
    }

    public void Position(string position, PositionType type)
    {
        RunCmd($"position {type.ToString().ToLower()} {position}");
    }

    public async Task<string?> Go(string? searchMoves = null, bool? ponder = null, int? wTime = null, int? bTime = null, int? wInc = null,
        int? bInc = null, int? movesToGo = null, int? depth = null, int? nodes = null, int? mate = null,
        int? moveTime = null, bool infinite = false)
    {
        var commands = new StringBuilder();
        if (searchMoves != null) commands.Append($"moves {searchMoves} ");
        if (ponder != null) commands.Append("ponder ");
        if (wTime != null) commands.Append($"wtime {wTime} ");
        if (bTime != null) commands.Append($"btime {bTime} ");
        if (wInc != null) commands.Append($"winc {wInc} ");
        if (bInc != null) commands.Append($"binc {bInc} ");
        if (movesToGo != null) commands.Append($"movestogo {movesToGo} ");
        if (depth != null) commands.Append($"depth {depth} ");
        if (nodes != null) commands.Append($"nodes {nodes} ");
        if (mate != null) commands.Append($"mate {mate} ");
        if (moveTime != null) commands.Append($"movetime {moveTime} ");
        
        RunCmd("go " + commands);
        return await WaitForBestMoveToken();
    }

    public async Task<string?> Stop()
    {
        RunCmd("stop");
        return await WaitForBestMoveToken();
    }

    public void PonderHit()
    {
        RunCmd("ponderhit");
    }

    public void Quit()
    {
        RunCmd("quit");
    }

    private void RunCmd(string command)
    {
        _process.StandardInput.WriteLine(command);
    }

    private async Task<string?> WaitForBestMoveToken()
    {
        var result = await WaitForStringOrToken("bestmove", 0);
        return string.IsNullOrEmpty(result) ? null : result.Split(' ')[1];
    }

    private async Task<string?> WaitForStringOrToken(string s, int tokenIndex = -1)
    {
        var val = await Task.Run(() =>
        {
            if (_process.StandardOutput == null) throw new Exception("Standard Output of the process is null!");
            
            while (!_process.StandardOutput.EndOfStream)
            {
                var str = _process.StandardOutput.ReadLine()!;
                if (string.IsNullOrEmpty(str)) break;

                Console.WriteLine($"[Fish Info]: {str}");
                if (tokenIndex >= 0)
                {
                    var tokens = str.Split(' ');
                    if (tokens[tokenIndex] == s) return str;
                }
                else
                {
                    if (s == str) return str;
                }
            }

            return null;
        });

        return val;
    }

    private static string GetStockfishBinaryFilePath()
    {
        if (System.OperatingSystem.IsWindows())
        {
            return "..\\..\\..\\..\\StockfishBinaries\\Windows\\stockfish_15_x64_avx2.exe";
        }
        
        if (System.OperatingSystem.IsLinux())
        {
            return "../../../../StockfishBinaries/Linux/stockfish_15_x64_avx2";
        }

        throw new SystemException("Unsupported OS detected!");
    }
}
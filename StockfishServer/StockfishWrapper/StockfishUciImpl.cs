using System.Diagnostics;
using System.Text;
using Domain.DTOs.Stockfish;
using Domain.Enums;

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

    public async Task<bool> IsReadyAsync()
    {
        RunCmd("isready");

        var result = await WaitForStringOrTokenAsync("readyok");
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

    public async Task<string?> GoAsync(StockfishGoDto dto)
    {
        var commands = new StringBuilder();
        if (dto.SearchMoves != null) commands.Append($"moves {dto.SearchMoves} ");
        if (dto.Ponder != null) commands.Append("ponder ");
        if (dto.WTime != null) commands.Append($"wtime {dto.WTime} ");
        if (dto.BTime != null) commands.Append($"btime {dto.BTime} ");
        if (dto.WInc != null) commands.Append($"winc {dto.WInc} ");
        if (dto.BInc != null) commands.Append($"binc {dto.BInc} ");
        if (dto.MovesToGo != null) commands.Append($"movestogo {dto.MovesToGo} ");
        if (dto.Depth != null) commands.Append($"depth {dto.Depth} ");
        if (dto.Nodes != null) commands.Append($"nodes {dto.Nodes} ");
        if (dto.Mate != null) commands.Append($"mate {dto.Mate} ");
        if (dto.MoveTime != null) commands.Append($"movetime {dto.MoveTime} ");

        RunCmd("go " + commands);
        return await WaitForBestMoveTokenAsync();
    }

    public async Task<string?> StopAsync()
    {
        RunCmd("stop");
        return await WaitForBestMoveTokenAsync();
    }

    public void PonderHit()
    {
        RunCmd("ponderhit");
    }

    public async Task<bool> SetOptionsAsync(StockfishSettingsDto settings)
    {
        SetOption("Threads", settings.Threads.ToString());
        SetOption("Hash", settings.Hash.ToString());
        SetOption("Ponder", settings.Ponder.ToString());
        SetOption("MultiPV", settings.MultiPv.ToString());
        SetOption("Skill Level", settings.SkillLevel.ToString());

        return await IsReadyAsync();
    }

    public void Quit()
    {
        RunCmd("quit");
    }

    private void RunCmd(string command)
    {
        _process.StandardInput.WriteLine(command);
    }

    private async Task<string?> WaitForBestMoveTokenAsync()
    {
        var result = await WaitForStringOrTokenAsync("bestmove", 0);
        return string.IsNullOrEmpty(result) ? null : result.Split(' ')[1];
    }

    private async Task<string?> WaitForStringOrTokenAsync(string s, int tokenIndex = -1)
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
            return "..\\StockfishBinaries\\Windows\\stockfish_15_x64_avx2.exe";
        }

        if (System.OperatingSystem.IsLinux())
        {
            return "../../../../StockfishBinaries/Linux/stockfish_15_x64_avx2";
        }

        throw new SystemException("Unsupported OS detected!");
    }
}
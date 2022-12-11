using Application.LogicInterfaces;
using Domain.DTOs.Stockfish;
using Domain.Enums;
using Domain.Models;
using Rudzoft.ChessLib.Exceptions;
using Rudzoft.ChessLib.Fen;
using StockfishWrapper;

namespace Application.Logic;

public class StockfishLogic : IStockfishLogic
{
    private readonly IStockfishUci _stockfish;

    public StockfishLogic(IStockfishUci stockfish)
    {
        _stockfish = stockfish;
    }

    public async Task<IFenData> GetBestMoveAsync(StockfishBestMoveDto dto)
    {
        try
        {
            Fen.Validate(dto.Fen);
        }
        catch (InvalidFen e)
        {
            Console.WriteLine(e);
            throw new ArgumentException("Invalid fen.");
        }

        if (!StockfishLevels.IsAi(dto.StockfishPlayer))
        {
            throw new ArgumentException("Invalid stockfish player.");
        }

        _stockfish.UciNewGame();
        _stockfish.Position(dto.Fen, PositionType.Fen);

        var levels = StockfishLevels.LevelOf[dto.StockfishPlayer];

        if (!await _stockfish.SetOptionsAsync(new StockfishSettingsDto() { SkillLevel = (int)levels.Skill }))
        {
            Console.WriteLine("[StockfishService]: Failed to set options!");
            throw new InvalidOperationException("Options failed to set.");
        }

        return new FenData(await _stockfish.GoAsync(new StockfishGoDto()
        {
            Depth = (int)levels.Depth,
            MoveTime = (int)levels.Time
        }));
    }

    public async Task<bool> IsReadyAsync()
    {
        return await _stockfish.IsReadyAsync();
    }
}
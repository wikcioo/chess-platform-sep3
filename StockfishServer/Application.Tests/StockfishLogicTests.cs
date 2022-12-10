using Application.Logic;
using Application.LogicInterfaces;
using Domain.DTOs.Stockfish;
using Moq;
using Rudzoft.ChessLib.Fen;
using StockfishWrapper;

namespace Application.Tests;

public class StockfishLogicTests
{
    private IStockfishLogic _stockfishLogic;

    public StockfishLogicTests()
    {
        var mock = new Mock<IStockfishUci>();
        mock.Setup(p => p.SetOptions(It.IsAny<StockfishSettingsDto>())).ReturnsAsync(true);
        mock.Setup(p => p.Go(new StockfishGoDto())).ReturnsAsync("");
        _stockfishLogic = new StockfishLogic(mock.Object);
    }

    [Fact]
    public async void GetBestMoveAsyncReturnsFenDataWhenValidFen()
    {
        var dto = new StockfishBestMoveDto("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "StockfishAi1");

        var result = await _stockfishLogic.GetBestMoveAsync(dto);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IFenData>(result);
    }

    [Fact]
    public async void GetBestMoveAsyncThrowsArgumentExceptionWhenInvalidFen()
    {
        var dto = new StockfishBestMoveDto("This is not a valid fen string", "StockfishAi1");


        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _stockfishLogic.GetBestMoveAsync(dto));

        Assert.Equal("Invalid fen.", ex.Message);
    }

    [Fact]
    public async void GetBestMoveAsyncThrowsArgumentExceptionWhenEmptyFen()
    {
        var dto = new StockfishBestMoveDto("", "StockfishAi1");


        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _stockfishLogic.GetBestMoveAsync(dto));

        Assert.Equal("Invalid fen.", ex.Message);
    }

    [Fact]
    public async void GetBestMoveAsyncThrowsArgumentExceptionWhenInvalidStockfishPlayer()
    {
        var dto = new StockfishBestMoveDto("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "This is not a valid stockfish player");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _stockfishLogic.GetBestMoveAsync(dto));

        Assert.Equal("Invalid stockfish player.", ex.Message);
    }

    [Fact]
    public async void GetBestMoveAsyncThrowsArgumentExceptionWhenEmptyStockfishPlayer()
    {
        var dto = new StockfishBestMoveDto("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _stockfishLogic.GetBestMoveAsync(dto));

        Assert.Equal("Invalid stockfish player.", ex.Message);
    }
}
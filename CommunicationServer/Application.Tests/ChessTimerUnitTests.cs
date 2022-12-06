using System.Threading;
using Application.ChessTimers;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Rudzoft.ChessLib.Enums;
using Xunit;

namespace Application.Tests;

public class ChessTimerUnitTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ReceivesCorrectAmountOfTimeUpdateEventsWithNoIncrement(int baseTime)
    {
        var counter = 0;
        
        var timer = new ChessTimer( (uint)baseTime, 0);
        timer.Elapsed += (_) => counter++;
        timer.StartTimers(false, true);
        
        Thread.Sleep(baseTime * 1000 + 500);
        Assert.Equal(baseTime, counter);
    }

    [Theory]
    [InlineData(2, 1, 2)]
    [InlineData(1, 2, 3)]
    [InlineData(3, 3, 1)]
    public void ReceivesCorrectAmountOfTimeUpdateEventsWithIncrement(int baseTime, int increment, int moves)
    {
        var counter = 0;
        
        var timer = new ChessTimer( (uint)baseTime, (uint)increment);
        timer.Elapsed += ( _) => counter++;
        timer.StartTimers(false, true);
        
        for (var i = 0; i < moves; i++)
        {
            timer.UpdateTimers(true);
        }
        
        Thread.Sleep(baseTime * 1000 + ((increment * 1000) * moves) + 500);
        Assert.Equal(baseTime, counter);
    }

    [Theory]
    [InlineData(6, 5, 5)]
    [InlineData(10, 1, 2)]
    [InlineData(35, 4, 3)]
    [InlineData(4564, 15, 31)]
    public void RemainingTimeIncrementsAfterMakingMovesWithIncrement(int baseTime, int increment, int moves)
    {
        var timer = new ChessTimer( (uint)baseTime, (uint)increment);
        
        for (var i = 0; i < moves; i++)
        {
            timer.UpdateTimers(true);
        }

        Assert.Equal(baseTime * 1000 + (increment * 1000) * moves, timer.WhiteRemainingTimeMs);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void ReturnsTimeIsUpGameEndTypeForLastEvent(int baseTime)
    {
        var timeEvent = new GameEventDto();
        
        var timer = new ChessTimer( (uint)baseTime, 0);
        timer.Elapsed += (dto) => timeEvent = dto;
        timer.StartTimers(false, true);

        Thread.Sleep(baseTime * 1000 + 500);
        Assert.Equal((uint)GameEndTypes.TimeIsUp, timeEvent.GameEndType);
    }

    // [Theory]
    // [InlineData(1)]
    // [InlineData(2)]
    // [InlineData(3)]
    // [InlineData(4)]
    // [InlineData(5)]
    // public void ReturnsCorrectSideThatLostOnTime(int baseTime)
    // {
    //     var timeEvent = new JoinedGameStreamDto();
    //     var whiteSidePlaying = true;
    //     var whiteTotalWaitTimeMs = 0;
    //     var blackTotalWaitTimeMs = 0;
    //     
    //     var timer = new ChessTimer(whiteSidePlaying, (uint)baseTime, 0);
    //     timer.Elapsed += (_, _, dto) => timeEvent = dto;
    //     timer.StartTimers();
    //
    //     while (timeEvent.GameEndType != (uint)GameEndTypes.TimeIsUp && whiteTotalWaitTimeMs < baseTime * 1000 && blackTotalWaitTimeMs < baseTime * 1000)
    //     {
    //         var timeToWaitMs = (int)(new Random().NextDouble() * 1000);
    //         Thread.Sleep(timeToWaitMs);
    //         
    //         if (whiteSidePlaying)
    //             whiteTotalWaitTimeMs += timeToWaitMs;
    //         else
    //             blackTotalWaitTimeMs += timeToWaitMs;
    //         
    //         if (whiteTotalWaitTimeMs >= baseTime * 1000 || blackTotalWaitTimeMs >= baseTime * 1000) break;
    //
    //         timer.UpdateTimers(whiteSidePlaying);
    //         whiteSidePlaying = !whiteSidePlaying;
    //     }
    //
    //     Assert.True(whiteTotalWaitTimeMs < blackTotalWaitTimeMs ? !timeEvent.IsWhite : timeEvent.IsWhite);
    // }
}
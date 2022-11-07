using Application.LogicImplementations;
using Domain.DTOs;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    bool OnWhiteSide { get; set; }
    bool IsDrawOfferPending { get; set; }
    public event GameLogic.StreamUpdate? TimeUpdated;
    public event GameLogic.StreamUpdate? NewFenReceived;
    public event GameLogic.StreamUpdate? ResignationReceived;
    public event GameLogic.StreamUpdate? InitialTimeReceived;
    public event GameLogic.StreamUpdate? DrawOffered;
    public event GameLogic.StreamUpdate? DrawOfferTimedOut;
    public event GameLogic.StreamUpdate? DrawOfferAccepted;
    
    public Task<ResponseGameDto> CreateGame(RequestGameDto dto);
    public Task JoinGame(RequestJoinGameDto dto);
}
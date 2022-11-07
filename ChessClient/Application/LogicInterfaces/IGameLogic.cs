using Application.LogicImplementations;
using Domain.DTOs;

namespace Application.LogicInterfaces;

public interface IGameLogic
{
    public event GameLogic.NextMove? GameStreamReceived;
    public Task<ResponseGameDto> CreateGame(RequestGameDto dto);
    public Task JoinGame(RequestJoinGameDto dto);
}
using Application.LogicInterfaces;
using Domain.DTOs.AuthorizedUserEvents;
using Domain.DTOs.GameEvents;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Hubs;

namespace WebAPI;

public class GroupHandler
{
    private readonly IHubContext<GameHub> _hubContext;

    public GroupHandler(IHubContext<GameHub> hubContext, IGameLogic gameLogic)
    {
        _hubContext = hubContext;
        gameLogic.GameEvent += FireGameEvent;
        gameLogic.AuthUserEvent += FireAuthUserEvent;
    }

    private void FireGameEvent(GameRoomEventDto dto)
    {
        _hubContext.Clients.Group(dto.GameRoomId.ToString()).SendAsync("GameStreamDto", dto.GameEventDto);
    }

    private void FireAuthUserEvent(AuthorizedUserEventDto dto)
    {
        _hubContext.Clients.User(dto.ReceiverUsername).SendAsync("AuthorizedUserEventDto", dto);
    }
}
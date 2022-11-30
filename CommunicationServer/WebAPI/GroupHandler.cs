using Application.Hubs;
using Application.LogicInterfaces;
using Domain.DTOs.GameEvents;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI;

public class GroupHandler
{
    private readonly IHubContext<GameHub> _hubContext;

    public GroupHandler(IHubContext<GameHub> hubContext, IGameLogic gameLogic)
    {
        _hubContext = hubContext;
        gameLogic.GameEvent += FireEvent;
    }


    private void FireEvent(GameRoomEventDto dto)
    {
        _hubContext.Clients.Group(dto.GameRoomId.ToString()).SendAsync("GameStreamDto", dto.GameEventDto);
    }
}
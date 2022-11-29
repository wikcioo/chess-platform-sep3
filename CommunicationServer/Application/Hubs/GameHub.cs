using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application.Hubs;

[Authorize]
public class GameHub : Hub<IGameHub>
{
    private readonly IChatLogic _chatLogic;
    private readonly IGameLogic _gameLogic;

    public GameHub(IChatLogic chatLogic, IGameLogic gameLogic)
    {
        _chatLogic = chatLogic;
        _gameLogic = gameLogic;
    }

    public async Task SendMessage(ulong gameRoom, string message)
    {
        var groupName = gameRoom.ToString();
        _chatLogic.Add(new MessageDto
        {
            Username = Context.User?.Identity?.Name,
            Body = message,
            GameRoom = gameRoom
        });
        await Clients.Group(groupName).ReceiveMessage(Context.User?.Identity?.Name, message);
    }

    public async Task JoinRoom(ulong gameRoom)
    {
        var groupName = gameRoom.ToString();
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Caller.GetLog(_chatLogic.GetLog(gameRoom));
    }

    public void JoinGame(RequestJoinGameDto dto)
    {
        dto.Username = Context.User?.Identity?.Name;
        var ack = _gameLogic.JoinGame(dto);
    }

    public async Task LeaveRoom(ulong gameRoom)
    {
        var groupName = gameRoom.ToString();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task MakeMove(MakeMoveDto dto)
    {
        dto.Username = Context.User?.Identity?.Name;
        var ack = await _gameLogic.MakeMove(dto);
    }

    public async Task Resign(RequestResignDto request)
    {
        request.Username = Context.User?.Identity?.Name;

        var ack = await _gameLogic.Resign(request);
        await Clients.Caller.ResignationResult(ack);
    }
}
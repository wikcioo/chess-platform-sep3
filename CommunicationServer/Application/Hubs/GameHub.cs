using Application.LogicInterfaces;
using Domain.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application.Hubs;

[Authorize]
public class GameHub : Hub<IGameHub>
{
    private readonly IChatLogic _chatLogic;

    public GameHub(IChatLogic chatLogic)
    {
        _chatLogic = chatLogic;
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


    public async Task LeaveRoom(ulong gameRoom)
    {
        var groupName = gameRoom.ToString();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
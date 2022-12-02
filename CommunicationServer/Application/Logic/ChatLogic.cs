using Application.LogicInterfaces;
using Domain.DTOs.Chat;
using Domain.Models;

namespace Application.Logic;

public class ChatLogic : IChatLogic
{
    private Dictionary<ulong, ChatRoom> _chatRooms = new();

    public void Add(MessageDto message)
    {
        var key = message.GameRoom;
        if (_chatRooms.ContainsKey(key))
        {
            ChatRoom chatRoom = _chatRooms[key];
            chatRoom.Add(message);
        }
        else
        {
            var newRoom = new ChatRoom();
            _chatRooms.Add(key, newRoom);
            newRoom.Add(message);
        }
    }

    public List<MessageDto> GetLog(ulong gameRoom)
    {
        return _chatRooms[gameRoom].GetLog();
    }


    public void StartChatRoom(ulong key)
    {
        _chatRooms[key] = new ChatRoom();
    }
}
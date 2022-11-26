using Application.Entities;
using Application.LogicInterfaces;
using Domain.DTOs;
using Domain.DTOs.Chat;

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

    public IObservable<MessageDto> GetMessagesAsObservable(RequestMessageDto request)
    {
        var key = request.GameRoom;
        if (_chatRooms.ContainsKey(key))
        {
            ChatRoom found = _chatRooms[key];
            return found.GetMessagedAsObservable();
        }

        var newRoom = new ChatRoom();
        _chatRooms.Add(key, newRoom);
        return newRoom.GetMessagedAsObservable();
    }

    public void StartChatRoom(ulong key)
    {
        _chatRooms[key] = new ChatRoom();
    }
}
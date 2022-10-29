using System.Reactive.Linq;

namespace GrpcService.Services;

public class ChatRoomService
{
    private Dictionary<string, ChatRoom> _chatRooms = new();

    public void Add(Message message)
    {
        string key = GenerateKey(message.Username, message.Receiver);
        Console.WriteLine(key);
        if (_chatRooms.ContainsKey(key))
        {
            ChatRoom chatRoom = _chatRooms[key];
            chatRoom.Add(message);
        }
        else
        {
            var newRoom = new ChatRoom(message.Username, message.Receiver);
            _chatRooms.Add(key, newRoom);
            newRoom.Add(message);
        }
    }

    public IObservable<Message> GetMessagesAsObservable(RequestMessage request)
    {
        string key = GenerateKey(request.Username, request.Receiver);
        if (_chatRooms.ContainsKey(key))
        {
            ChatRoom found = _chatRooms[key];
            return found.GetMessagedAsObservable();
        }

        var newRoom = new ChatRoom(request.Username, request.Receiver);
        _chatRooms.Add(key, newRoom);
        return newRoom.GetMessagedAsObservable();
    }

    private static string GenerateKey(string senderUsername, string receiverUsername)
    {
        if (String.CompareOrdinal(senderUsername, receiverUsername) < 0)
        {
            return senderUsername + receiverUsername;
        }

        return receiverUsername + senderUsername;
    }
}
using Domain.Enums;

namespace Application.Entities;

public class GameRoomsData
{
    private static ulong _nextGameId = 1;

    private readonly Dictionary<ulong, GameRoom> _gameRooms = new();

    public ulong Add(GameRoom gameRoom)
    {
        var id = _nextGameId++;
        gameRoom.Id = id;
        _gameRooms.Add(id, gameRoom);
        return id;
    }

    public GameRoom Get(ulong id)
    {
        if (_gameRooms.ContainsKey(id))
            return _gameRooms[id];

        throw new KeyNotFoundException();
    }

    public void Remove(ulong id)
    {
        _gameRooms.Remove(id);
    }

    public bool CanUsernameJoin(GameRoom room, string username)
    {
        if (room.GameType is OpponentTypes.Random or OpponentTypes.Ai)
            return true;

        return room.PlayerWhite!.Equals(username) || room.PlayerBlack!.Equals(username);
    }

    public IEnumerable<GameRoom> GetJoinableByUsername(string requesterUsername)
    {
        return _gameRooms.Select(pair => pair.Value)
            .Where(room => room.IsJoinable && CanUsernameJoin(room, requesterUsername));
    }

    public IEnumerable<GameRoom> GetSpectateable()
    {
        return _gameRooms.Select(pair => pair.Value).Where(room => room.IsSpectatable).ToList();
    }
}
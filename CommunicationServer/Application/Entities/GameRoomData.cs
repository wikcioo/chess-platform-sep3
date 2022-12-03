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

    public bool Add(GameRoom gameRoom, ulong index)
    {
        var ret = _gameRooms.ContainsKey(index);
        _gameRooms.Add(index, gameRoom);
        return ret;
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

    public IEnumerable<GameRoom> GetAll()
    {
        return _gameRooms.Select(pair => pair.Value).ToList();
    }
}
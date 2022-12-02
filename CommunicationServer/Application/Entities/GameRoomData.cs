using Domain.Enums;

namespace Application.Entities;

public class GameRoomsData
{
    private static ulong _nextGameId = 1;

    private readonly Dictionary<ulong, GameRoomHandler> _gameRooms = new();

    public ulong Add(GameRoomHandler gameRoomHandler)
    {
        var id = _nextGameId++;
        gameRoomHandler.Id = id;
        _gameRooms.Add(id, gameRoomHandler);
        return id;
    }

    public GameRoomHandler Get(ulong id)
    {
        if (_gameRooms.ContainsKey(id))
            return _gameRooms[id];

        throw new KeyNotFoundException();
    }

    public void Remove(ulong id)
    {
        _gameRooms.Remove(id);
    }
    public IEnumerable<GameRoomHandler> GetAll()
    {
        return _gameRooms.Select(pair => pair.Value).ToList();
    }
}
using Domain.Enums;

namespace Application.Entities;

public class GameRoomsData
{
    private static readonly Stack<ulong> RecentlyFreedGameIds = new();
    private static ulong _nextGameId = 0;
    
    private readonly Dictionary<ulong, GameRoom> _gameRooms = new();
    private readonly Dictionary<ulong, bool> _visible = new();
    private readonly Dictionary<ulong, OpponentTypes> _opponentType = new();
    private readonly List<ulong> _spectateable = new();
    private readonly List<ulong> _joinable = new();

    public readonly Dictionary<ulong, uint> NumPlayersJoined = new();
    public readonly Dictionary<ulong, uint> NumSpectatorsJoined = new();

    private static ulong GenerateNextGameId()
    {
        return RecentlyFreedGameIds.Count > 0 ? RecentlyFreedGameIds.Pop() : _nextGameId++;
    }

    public ulong Add(GameRoom gameRoom, bool isVisible, OpponentTypes opponentType)
    {
        var id = GenerateNextGameId();
        _gameRooms.Add(id, gameRoom);
        _visible.Add(id, isVisible);
        _opponentType.Add(id, opponentType);
        _joinable.Add(id);
        NumPlayersJoined[id] = 0;
        NumSpectatorsJoined[id] = 0;
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
        _visible.Remove(id);
        _opponentType.Remove(id);
        _spectateable.Remove(id);
        _joinable.Remove(id);
        NumPlayersJoined[id] = 0;
        NumSpectatorsJoined[id] = 0;
        RecentlyFreedGameIds.Push(id);
    }

    public void TransitionFromJoinableAndAddToSpectateableIfVisible(ulong id)
    {
        if (_visible[id])
            _spectateable.Add(id);
        _joinable.Remove(id);
    }

    public bool IsJoinable(ulong id)
    {
        return _joinable.Contains(id);
    }

    public bool CanUsernameJoin(ulong id, string username)
    {
        if (_opponentType[id] == OpponentTypes.Random || _opponentType[id] == OpponentTypes.Ai)
            return true;
        
        if (_gameRooms[id].PlayerWhite!.Equals(username) || _gameRooms[id].PlayerBlack!.Equals(username))
            return true;
        
        return false;
    }

    public List<Tuple<ulong, GameRoom>> GetJoinable()
    {
        var gameRooms = new List<Tuple<ulong, GameRoom>>();
        foreach (var id in _joinable)
        {
            gameRooms.Add(new Tuple<ulong, GameRoom>(id, _gameRooms[id]));
        }

        return gameRooms;
    }

    public bool IsSpectateable(ulong id)
    {
        if (_visible[id])
            return _spectateable.Contains(id);
        return false;
    }

    public List<Tuple<ulong, GameRoom>> GetSpectateable()
    {
        var gameRooms = new List<Tuple<ulong, GameRoom>>();
        foreach (var id in _spectateable)
        {
            if (_visible[id])
                gameRooms.Add(new Tuple<ulong, GameRoom>(id, _gameRooms[id]));
        }
        
        return gameRooms;
    }
}
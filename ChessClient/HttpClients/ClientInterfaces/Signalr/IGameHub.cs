using Domain.DTOs.GameEvents;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IGameHub
{
    public event Action<GameEventDto>? GameEventReceived;
    void StartListeningToGameEvents();
    Task LeaveRoom(ulong? gameRoomId);
    Task JoinRoom(ulong? gameRoomId);
}
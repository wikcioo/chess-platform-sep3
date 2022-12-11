using Domain.DTOs.GameEvents;

namespace HttpClients.ClientInterfaces.Signalr;

public interface IGameHub
{
    event Action<GameEventDto>? GameEventReceived;
    void StartListeningToGameEvents();
    Task LeaveRoomAsync(ulong? gameRoomId);
    Task JoinRoomAsync(ulong? gameRoomId);
}
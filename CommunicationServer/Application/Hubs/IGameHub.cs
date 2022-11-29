using Domain.DTOs;
using Domain.DTOs.Chat;
using Domain.DTOs.GameRoomData;
using Domain.Enums;

namespace Application.Hubs;

public interface IGameHub
{
    Task ReceiveMessage(string username, string body);
    Task GetLog(List<MessageDto> log);
    Task ResignationResult(AckTypes ack);
    Task ReceiveJoinableGames(IEnumerable<JoinableGameRoomDataDto> rooms);
    Task ReceiveGameEvent(JoinedGameStreamDto dto);
}
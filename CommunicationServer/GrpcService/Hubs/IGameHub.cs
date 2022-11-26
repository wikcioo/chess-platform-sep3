using Domain.DTOs.Chat;

namespace GrpcService.Hubs;

public interface IGameHub
{
    Task ReceiveMessage(string username, string body);
    Task GetLog(List<MessageDto> log);
}
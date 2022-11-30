using Domain.DTOs.Chat;

namespace Application.Hubs;

public interface IGameHub
{
    Task ReceiveMessage(string username, string body);
    Task GetLog(List<MessageDto> log);
}
using Domain.DTOs.Chat;

namespace Application.Hubs;

public interface IGameHub
{
    Task ReceiveMessage(MessageDto dto);
    Task GetLog(List<MessageDto> log);
}
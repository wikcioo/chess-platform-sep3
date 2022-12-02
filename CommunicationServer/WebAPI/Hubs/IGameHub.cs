using Domain.DTOs.Chat;

namespace WebAPI.Hubs;

public interface IGameHub
{
    Task ReceiveMessage(MessageDto dto);
    Task GetLog(List<MessageDto> log);
}
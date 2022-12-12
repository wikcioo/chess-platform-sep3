using Domain.DTOs.Game;

namespace Application.ClientInterfaces;

public interface IGameService
{
    Task CreateAsync(GameCreationDto dto);
}
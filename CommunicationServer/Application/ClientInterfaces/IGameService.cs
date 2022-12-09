using Domain.DTOs;
using Rudzoft.ChessLib;

namespace Application.ClientInterfaces;

public interface IGameService
{
    Task CreateAsync(GameCreationDto dto);
}
using HttpClients.Implementations;

namespace HttpClients.ClientInterfaces;

public interface IAuthUserService
{
    public event AuthUserService.StreamUpdate? NewGameOffer;
    
    public Task JoinUserEvents();
}
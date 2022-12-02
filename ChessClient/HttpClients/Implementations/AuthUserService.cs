using System.Net.Http.Headers;
using Domain.DTOs.AuthorizedUserEvents;
using Domain.Enums;
using HttpClients.ClientInterfaces;
using HttpClients.Signalr;
using Microsoft.AspNetCore.SignalR.Client;

namespace HttpClients.Implementations;

public class AuthUserService : IAuthUserService
{
    private readonly IAuthService _authService;
    private readonly HubConnectionWrapper _hubDto;
    private readonly HttpClient _client;
    
    public delegate void StreamUpdate(AuthorizedUserEventDto dto);
    public event StreamUpdate? NewGameOffer;
    
    public AuthUserService(IAuthService authService, HubConnectionWrapper hubDto, HttpClient client)
    {
        _authService = authService;
        _hubDto = hubDto;
        _client = client;
    }

    public async Task JoinUserEvents()
    {
        _hubDto.HubConnection?.Remove("AuthorizedUserEventDto");
        _hubDto.HubConnection?.On<AuthorizedUserEventDto>("AuthorizedUserEventDto", ListenToAuthorizedUserEvents);
        
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");
    }

    private void ListenToAuthorizedUserEvents(AuthorizedUserEventDto dto)
    {
        switch (dto.Event)
        {
            case AuthUserEvents.NewGameOffer:
                NewGameOffer?.Invoke(dto);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
using System.Security.Claims;
using HttpClients.ClientInterfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWASM.Auth;

public class CustomAuthProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;

    public CustomAuthProvider(IAuthService authService)
    {
        _authService = authService;
        authService.OnAuthStateChanged += AuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = await _authService.GetAuthAsync();
        return new AuthenticationState(principal);
    }

    private void AuthStateChanged(ClaimsPrincipal principal)
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(principal)
            )
        );
    }
}
using System.Security.Claims;
using Domain.Models;

namespace HttpClients.ClientInterfaces;

public interface IAuthService
{
    Task LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<ClaimsPrincipal> GetAuthAsync();

    Action<ClaimsPrincipal> OnAuthStateChanged { get; set; }

    string GetJwtToken();
}
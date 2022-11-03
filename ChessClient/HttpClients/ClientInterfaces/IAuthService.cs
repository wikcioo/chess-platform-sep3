using System.Security.Claims;
using Domain.Models;

namespace HttpClients.ClientInterfaces;

public interface IAuthService
{
    public Task LoginAsync(string email, string password);
    public Task LogoutAsync();
    public Task<ClaimsPrincipal> GetAuthAsync();

    public Action<ClaimsPrincipal> OnAuthStateChanged { get; set; }

    public string GetJwtToken();
}
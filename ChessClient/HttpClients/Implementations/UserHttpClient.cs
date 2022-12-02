using System.Net.Http.Json;
using System.Text.Json;
using Domain.Models;
using HttpClients.ClientInterfaces;
using System.Net.Http.Headers;
using Domain.DTOs;

namespace HttpClients.Implementations;

public class UserHttpClient : IUserService
{
    private readonly HttpClient _client;
    private readonly IAuthService _authService;

    public UserHttpClient(HttpClient client, IAuthService authService)
    {
        _client = client;
        _authService = authService;
    }

    public async Task<User> CreateAsync(User user)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var response = await _client.PostAsJsonAsync("/users", user);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Network error. Failed to create a user.");
        }

        var created = JsonSerializer.Deserialize<User>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return created;
    }
    
    public async Task<IEnumerable<UserSearchResultDto>> GetAsync(UserSearchParamsDto paramsDto)
    {
        var query = ConstructQuery(paramsDto.Username);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        HttpResponseMessage response = await _client.GetAsync("/users"+query);
        string result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Network error. Failed to get users.");
        }

        ICollection<UserSearchResultDto> users = JsonSerializer.Deserialize<ICollection<UserSearchResultDto>>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return users;
    }
    
    private static string ConstructQuery(string? userName)
    {
        string query = "";
        if (!string.IsNullOrEmpty(userName))
        {
            query += $"?username={userName}";
        }
        
        return query;
    }
}
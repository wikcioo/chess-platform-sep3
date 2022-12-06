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

        try
        {
            var response = await _client.PostAsJsonAsync("/users", user);
            return await ResponseParser.ParseAsync<User>(response);
        }
        catch (HttpRequestException e)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, object>>(e.Message);
            if (values?["message"] != null)
                throw new HttpRequestException(values["message"].ToString(), e);
            throw new HttpRequestException("Network error. Failed to create a user.", e);
        }
    }

    public async Task<IEnumerable<UserSearchResultDto>> GetAsync(UserSearchParamsDto paramsDto)
    {
        var query = ConstructQuery(paramsDto.Username);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        try
        {
            HttpResponseMessage response = await _client.GetAsync("/users" + query);
            return await ResponseParser.ParseAsync<IEnumerable<UserSearchResultDto>>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to get users.", e);
        }
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
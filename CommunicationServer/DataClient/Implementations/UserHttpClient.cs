using System.Net.Http.Json;
using System.Text.Json;
using Application.ClientInterfaces;
using Domain.DTOs;
using Domain.Models;

namespace DatabaseClient.Implementations;

public class UserHttpClient : IUserService
{
    private readonly HttpClient _client;

    public UserHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<User> LoginAsync(UserLoginDto dto)
    {
        var response = await _client.PostAsJsonAsync("/login", dto);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }
        var existing = JsonSerializer.Deserialize<User>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return existing;
    }

    public async Task<User> CreateAsync(User user)
    {
        var response = await _client.PostAsJsonAsync("/users", user);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }

        var created = JsonSerializer.Deserialize<User>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return created;
    }

    public async Task<IEnumerable<User>> GetAsync(UserSearchParamsDto paramsDto)
    {
        var query = ConstructQuery(paramsDto.Username);
        
        HttpResponseMessage response = await _client.GetAsync("/users"+query);
        string result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }

        ICollection<User> users = JsonSerializer.Deserialize<ICollection<User>>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        return users;
    }
    
    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (username == string.Empty)
        {
            return null;
        }
        
        HttpResponseMessage response = await _client.GetAsync($"/users/{username}");
        string result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(result);
        }

        var users = JsonSerializer.Deserialize<User?>(result, new JsonSerializerOptions
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
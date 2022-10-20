using System.Net.Http.Json;
using Application.DaoInterfaces;
using Domain.DTOs;

namespace DatabaseClient.Implementations;

public class UserHttpClient : IUserService
{
    private readonly HttpClient _client;

    public UserHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<bool> LoginAsync(UserLoginDto dto)
    {
        var response = await _client.PostAsJsonAsync("/login", dto);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }

        return Boolean.Parse(result);
    }
}
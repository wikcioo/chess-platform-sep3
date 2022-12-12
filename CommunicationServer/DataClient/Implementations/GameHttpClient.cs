using System.Net.Http.Json;
using Application.ClientInterfaces;
using Domain.DTOs.Game;

namespace DatabaseClient.Implementations;

public class GameHttpClient : IGameService
{
    private readonly HttpClient _client;

    public GameHttpClient(HttpClient client)
    {
        _client = client;
    }

    public async Task CreateAsync(GameCreationDto dto)
    {
        var response = await _client.PostAsJsonAsync("/games", dto);
        var result = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(result);
        }
    }
}
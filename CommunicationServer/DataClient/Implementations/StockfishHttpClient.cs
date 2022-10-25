using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Application.ClientInterfaces;
using Domain.DTOs;

namespace DatabaseClient.Implementations;

public class StockfishHttpClient : IStockfishService
{
    private readonly HttpClient _client;

    public StockfishHttpClient(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new Uri("https://localhost:7007");
    }

    public async Task<bool> GetStockfishIsReadyAsync()
    {
        var response = await _client.GetAsync("/ready");
        var result = await response.Content.ReadAsStringAsync();
        return result.Equals("true");
    }

    public async Task<string> GetBestMoveAsync(StockfishBestMoveDto dto)
    {
        var response = await _client.PostAsJsonAsync("/bestMove", dto);
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}
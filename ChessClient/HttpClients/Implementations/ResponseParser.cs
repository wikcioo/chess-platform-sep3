using System.Text.Json;

namespace HttpClients.Implementations;

public static class ResponseParser
{
    public static async Task<T> ParseAsync<T>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        return JsonSerializer.Deserialize<T>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
    }
}
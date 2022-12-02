using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using HttpClients.ClientInterfaces;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.WebUtilities;
using Rudzoft.ChessLib.Types;

namespace HttpClients.Implementations;

public class GameService : IGameService
{
    private readonly IAuthService _authService;
    public bool IsDrawOfferPending { get; set; } = false;
    public bool OnWhiteSide { get; set; } = true;
    public ulong? GameRoomId { get; set; }


    //Todo Possibility of replacing StreamUpdate with action and only needed information instead of dto
    public delegate void StreamUpdate(GameEventDto dto);

    public event StreamUpdate? TimeUpdated;
    public event StreamUpdate? NewFenReceived;
    public event StreamUpdate? ResignationReceived;
    public event StreamUpdate? NewPlayerJoined;
    public event StreamUpdate? DrawOffered;
    public event StreamUpdate? DrawOfferTimedOut;
    public event StreamUpdate? DrawOfferAccepted;
    public event StreamUpdate? EndOfTheGameReached;
    public event Action? GameFirstJoined;

    public event Action<CurrentGameStateDto>? StateReceived;

    //Signalr
    private readonly IGameHub _gameHub;
    private HttpClient _client;

    public GameService(IAuthService authService, IGameHub gameHub, HttpClient client)
    {
        _authService = authService;
        _gameHub = gameHub;
        _client = client;
        gameHub.GameEventReceived += ListenToGameEvents;
    }

    public async Task<ResponseGameDto> CreateGame(RequestGameDto dto)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");
        dto.Username = user.Identity!.Name!;
        var response = await _client.PostAsJsonAsync("/games", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var created = JsonSerializer.Deserialize<ResponseGameDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        OnWhiteSide = created.IsWhite;
        return created;
    }

    public async Task JoinGame(RequestJoinGameDto dto)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");

        _gameHub.StartListeningToGameEvents();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var response = await _client.PostAsync($"/games/{dto.GameRoom}/users", null);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        GameRoomId = dto.GameRoom;

        await _gameHub.JoinRoom(GameRoomId);
    }


    private void ListenToGameEvents(GameEventDto response)
    {
        switch (response.Event)
        {
            case GameStreamEvents.NewFenPosition:
                NewFenReceived?.Invoke(response);
                TimeUpdate(response);
                break;
            case GameStreamEvents.TimeUpdate:
                // if (response.GameEndType == (uint) GameEndTypes.TimeIsUp) _call.Dispose();
                TimeUpdate(response);
                break;
            case GameStreamEvents.ReachedEndOfTheGame:
                // _call.Dispose();
                TimeUpdate(response);
                NewFenReceived?.Invoke(response);
                EndOfTheGameReached?.Invoke(response);
                break;
            case GameStreamEvents.Resignation:
                // _call.Dispose();
                ResignationReceived?.Invoke(response);
                break;
            case GameStreamEvents.DrawOffer:
                DrawOffer(response);
                break;
            case GameStreamEvents.DrawOfferTimeout:
                DrawOfferTimeout(response);
                break;
            case GameStreamEvents.DrawOfferAcceptation:
                // _call.Dispose();
                DrawOfferAcceptation(response);
                break;
            case GameStreamEvents.PlayerJoined:
                // _call.Dispose();
                PlayerJoined(response);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void TimeUpdate(GameEventDto dto)
    {
        TimeUpdated?.Invoke(dto);
    }

    public async Task GetCurrentGameState()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var response = await _client.GetAsync($"/games/{GameRoomId.Value}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var streamDto = JsonSerializer.Deserialize<CurrentGameStateDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;


        var myName = user.Identity!.Name!;
        if (streamDto.UsernameBlack.Equals(myName))
        {
            OnWhiteSide = false;
        }

        if (streamDto.UsernameWhite.Equals(myName))
        {
            OnWhiteSide = true;
        }

        StateReceived?.Invoke(streamDto);
        GameFirstJoined?.Invoke();
    }

    public void LeaveRoom()
    {
        _gameHub.LeaveRoom(GameRoomId);
    }

    private async void DrawOffer(GameEventDto dto)
    {
        var user = await _authService.GetAuthAsync();
        if (dto.UsernameWhite.Equals(user.Identity!.Name) || dto.UsernameBlack.Equals(user.Identity!.Name))
            IsDrawOfferPending = true;

        DrawOffered?.Invoke(dto);
    }

    private void DrawOfferTimeout(GameEventDto dto)
    {
        IsDrawOfferPending = false;
        DrawOfferTimedOut?.Invoke(dto);
    }

    private void DrawOfferAcceptation(GameEventDto dto)
    {
        IsDrawOfferPending = false;
        DrawOfferAccepted?.Invoke(dto);
    }

    public async Task<AckTypes> MakeMove(Move move)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new MakeMoveDto
        {
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            GameRoom = GameRoomId.Value,
            MoveType = (uint) move.MoveType(),
            Promotion = (uint) move.PromotedPieceType().AsInt(),
            Username = user.Identity!.Name!
        };
        var response = await _client.PostAsJsonAsync("/moves", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public async Task<AckTypes> OfferDraw()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new RequestDrawDto()
        {
            GameRoom = GameRoomId.Value
        };
        var response = await _client.PostAsJsonAsync("/draw-offers", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public void PlayerJoined(GameEventDto dto)
    {
        GameFirstJoined?.Invoke();
        NewPlayerJoined?.Invoke(dto);
    }

    public async Task<AckTypes> Resign()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new ResponseDrawDto
        {
            GameRoom = GameRoomId.Value
        };
        var response = await _client.PostAsJsonAsync("/resignation", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public async Task<AckTypes> SendDrawResponse(bool accepted)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new ResponseDrawDto
        {
            GameRoom = GameRoomId.Value,
            Accept = accepted
        };
        var response = await _client.PostAsJsonAsync("/draw-responses", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public async Task<IList<GameRoomDto>> GetGameRooms(GameRoomSearchParameters parameters)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var queryParams = new Dictionary<string, string>();

        if (parameters.Spectateable)
        {
            queryParams.Add("spectateable", parameters.Spectateable.ToString());
        }

        if (parameters.Joinable)
        {
            queryParams.Add("joinable", parameters.Spectateable.ToString());
        }

        var uri = QueryHelpers.AddQueryString("/rooms", queryParams);

        var response = await _client.GetAsync(uri);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent);
        }

        var roomList = JsonSerializer.Deserialize<IEnumerable<GameRoomDto>>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return roomList.ToList();
    }
}
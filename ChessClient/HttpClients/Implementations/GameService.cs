using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.DTOs;
using Domain.DTOs.Chat;
using Domain.DTOs.GameEvents;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using HttpClients.ClientInterfaces;
using HttpClients.Signalr;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.WebUtilities;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;

namespace HttpClients.Implementations;

public class GameService : IGameService
{
    private readonly IAuthService _authService;
    public bool IsDrawOfferPending { get; set; }
    public bool OnWhiteSide { get; set; } = true;
    public ulong? GameRoomId { get; set; }

    public string LastFen { get; set; } = Fen.StartPositionFen;


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

    private readonly HubConnectionWrapper _hubDto;
    private readonly HttpClient _client;

    public GameService(IAuthService authService, HubConnectionWrapper hubDto, HttpClient client)
    {
        _authService = authService;
        _hubDto = hubDto;
        _client = client;
    }

    public async Task StartHubConnection()
    {
        try
        {
            if (_hubDto.HubConnection is not null)
            {
                await _hubDto.HubConnection.DisposeAsync();
            }

            _hubDto.HubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7233/gamehub",
                    options => { options.AccessTokenProvider = () => Task.FromResult(_authService.GetJwtToken())!; })
                .WithAutomaticReconnect()
                .Build();
            //Required so the connection is not dropped
            _hubDto.HubConnection.On<string>("DummyConnection", _ => { });
            await _hubDto.HubConnection.StartAsync();
        }
        catch (HttpRequestException)
        {
            throw new HttpRequestException("Network error. Failed to connect to a stream");
        }
    }
    
    public Task<string> GetLastFen()
    {
        return Task.FromResult(LastFen);
    }

    public async void LeaveRoom()
    {
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.SendAsync("LeaveRoom", GameRoomId);
        }
    }

    public async Task StopHubConnection()
    {
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.StopAsync();
            await _hubDto.HubConnection.DisposeAsync();
            _hubDto.HubConnection = null;
        }
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

        try
        {
            var response = await _client.PostAsJsonAsync("/games", dto);
            var created = await ParseResponse<ResponseGameDto>(response);
            OnWhiteSide = created.IsWhite;
            return created;
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to create the game", e);
        }
    }

    public async Task JoinGame(RequestJoinGameDto dto)
    {
        _hubDto.HubConnection?.Remove("GameStreamDto");
        _hubDto.HubConnection?.On<GameEventDto>("GameStreamDto",
            ListenToJoinedGameStream);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");

        try
        {
            var response = await _client.PostAsync($"/games/{dto.GameRoom}/users", null);
            var ack = await ParseResponse<AckTypes>(response);

            if(ack != AckTypes.Success)
                throw new HttpRequestException($"Ack code: {ack}");
            
            GameRoomId = dto.GameRoom;

            if (_hubDto.HubConnection is not null)
            {
                await _hubDto.HubConnection.SendAsync("JoinRoom", GameRoomId);
            }
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to join the game", e);
        }
    }


    private void ListenToJoinedGameStream(GameEventDto response)
    {
        switch (response.Event)
        {
            case GameStreamEvents.NewFenPosition:
                NewFen(response);
                break;
            case GameStreamEvents.TimeUpdate:
                TimeUpdate(response);
                break;
            case GameStreamEvents.ReachedEndOfTheGame:
                EndOfTheGame(response);
                break;
            case GameStreamEvents.Resignation:
                ResignationReceived?.Invoke(response);
                break;
            case GameStreamEvents.DrawOffer:
                DrawOffer(response);
                break;
            case GameStreamEvents.DrawOfferTimeout:
                DrawOfferTimeout(response);
                break;
            case GameStreamEvents.DrawOfferAcceptation:
                DrawOfferAcceptation(response);
                break;
            case GameStreamEvents.PlayerJoined:
                PlayerJoined(response);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void TimeUpdate(GameEventDto dto)
    {
        TimeUpdated?.Invoke(dto);
    }

    private void EndOfTheGame(GameEventDto dto)
    {
        TimeUpdate(dto);
        NewFenReceived?.Invoke(dto);
        EndOfTheGameReached?.Invoke(dto);
    }

    private void NewFen(GameEventDto dto)
    {
        NewFenReceived?.Invoke(dto);
        LastFen = dto.FenString;
        TimeUpdate(dto);
    }

    public async Task GetCurrentGameState()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        try
        {
            var response = await _client.GetAsync($"/games/{GameRoomId.Value}");
            var streamDto = await ParseResponse<CurrentGameStateDto>(response);

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
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to get current game state", e);
        }
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
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (_hubDto.HubConnection == null)
        {
            throw new InvalidOperationException("You are not logged in!");
        }

        var user = await _authService.GetAuthAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new MakeMoveDto
        {
            FromSquare = move.FromSquare().ToString(),
            ToSquare = move.ToSquare().ToString(),
            GameRoom = GameRoomId.Value,
            MoveType = (uint)move.MoveType(),
            Promotion = (uint)move.PromotedPieceType().AsInt(),
            Username = user.Identity!.Name!
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/moves", dto);
            return await ParseResponse<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to make a move.", e);
        }
    }

    public async Task<AckTypes> OfferDraw()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new RequestDrawDto()
        {
            Username = user.Identity!.Name!,
            GameRoom = GameRoomId.Value
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/draw-offers", dto);
            return await ParseResponse<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to offer a draw.", e);
        }
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
            Username = user.Identity!.Name!,
            GameRoom = GameRoomId.Value
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/resignation", dto);
            return await ParseResponse<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to resign from the game.", e);
        }
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

        try
        {
            var response = await _client.PostAsJsonAsync("/draw-responses", dto);
            return await ParseResponse<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to respond to a draw offer.", e);
        }
    }

    public static async Task<T> ParseResponse<T>(HttpResponseMessage response)
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
            queryParams.Add("joinable", parameters.Joinable.ToString());
        }

        var uri = QueryHelpers.AddQueryString("/rooms", queryParams);
        try
        {
            var response = await _client.GetAsync(uri);
            var roomList = await ParseResponse<IEnumerable<GameRoomDto>>(response);
            return roomList.ToList();
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to retrieve game rooms.", e);
        }
    }
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.DTOs;
using Domain.DTOs.GameEvents;
using Domain.Enums;
using HttpClients.ClientInterfaces;
using HttpClients.ClientInterfaces.Signalr;
using Microsoft.AspNetCore.WebUtilities;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.Types;

namespace HttpClients.Implementations;

public class GameService : IGameService
{
    private readonly IAuthService _authService;
    public bool IsDrawOfferPending { get; set; }
    public bool IsRematchOfferRequestPending { get; set; }
    public bool IsRematchOfferResponsePending { get; set; }
    public bool OnWhiteSide { get; set; } = true;
    public bool Spectating { get; set; }
    public ulong? GameRoomId { get; set; }
    public string LastFen { get; set; } = Fen.StartPositionFen;
    public RequestGameDto? RequestedGameDto { get; set; }

    public event Action<GameEventDto>? TimeUpdated;
    public event Action<GameEventDto>? NewFenReceived;
    public event Action<GameEventDto>? ResignationReceived;
    public event Action<GameEventDto>? NewPlayerJoined;
    public event Action<GameEventDto>? DrawOffered;
    public event Action<GameEventDto>? DrawOfferTimedOut;
    public event Action<GameEventDto>? DrawOfferAccepted;
    public event Action<GameEventDto>? RematchOffered;
    public event Action<GameEventDto>? RematchOfferTimedOut;
    public event Action<GameEventDto>? RematchOfferAccepted;
    public event Action<GameEventDto>? EndOfTheGameReached;
    public event Action<GameEventDto>? JoinRematchedGame;
    public event Action<GameEventDto>? GameAborted;
    public event Action? GameFirstJoined;

    public event Action<CurrentGameStateDto>? StateReceived;

    private readonly IGameHub _gameHub;
    private readonly HttpClient _client;

    public GameService(IAuthService authService, IGameHub gameHub, HttpClient client)
    {
        _authService = authService;
        _gameHub = gameHub;
        _client = client;
        gameHub.GameEventReceived += ListenToGameEvents;
    }

    public Task<string> GetLastFenAsync()
    {
        return Task.FromResult(LastFen);
    }

    private void PlayerJoined(GameEventDto dto)
    {
        GameFirstJoined?.Invoke();
        NewPlayerJoined?.Invoke(dto);
    }

    public async Task<ResponseGameDto> CreateGameAsync(RequestGameDto dto)
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
            var created = await ResponseParser.ParseAsync<ResponseGameDto>(response);
            OnWhiteSide = created.IsWhite;
            return created;
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to create the game", e);
        }
    }

    public async Task JoinGameAsync(RequestJoinGameDto dto)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _gameHub.StartListeningToGameEvents();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        try
        {
            var response = await _client.PostAsync($"/games/{dto.GameRoom}/users", null);
            var ack = await ResponseParser.ParseAsync<AckTypes>(response);

            if (ack != AckTypes.Success)
                throw new HttpRequestException($"Ack code: {ack}");

            LeaveRoomAsync();
            GameRoomId = dto.GameRoom;
            await _gameHub.JoinRoomAsync(GameRoomId);
        }

        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to join the game", e);
        }
    }

    public async Task SpectateGameAsync(RequestJoinGameDto dto)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _gameHub.StartListeningToGameEvents();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        try
        {
            var response = await _client.PostAsync($"/games/{dto.GameRoom}/spectators", null);
            var ack = await ResponseParser.ParseAsync<AckTypes>(response);

            if (ack != AckTypes.Success)
                throw new HttpRequestException($"Ack code: {ack}");

            GameRoomId = dto.GameRoom;
            await _gameHub.JoinRoomAsync(GameRoomId);
        }

        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to spectate the game", e);
        }
    }

    private void ListenToGameEvents(GameEventDto response)
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
            case GameStreamEvents.RematchOffer:
                RematchOffer(response);
                break;
            case GameStreamEvents.RematchOfferTimeout:
                RematchOfferTimeout(response);
                break;
            case GameStreamEvents.RematchOfferAcceptation:
                RematchOfferAcceptation(response);
                break;
            case GameStreamEvents.RematchInvitation:
                JoinRematchGame(response);
                break;
            case GameStreamEvents.PlayerJoined:
                PlayerJoined(response);
                break;
            case GameStreamEvents.GameAborted:
                AbortGame(response);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void AbortGame(GameEventDto dto)
    {
        GameAborted?.Invoke(dto);
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

    public async Task GetCurrentGameStateAsync()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");
        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        try
        {
            var response = await _client.GetAsync($"/games/{GameRoomId.Value}");
            var streamDto = await ResponseParser.ParseAsync<CurrentGameStateDto>(response);

            LastFen = streamDto.FenString;
            var myName = user.Identity!.Name!;
            if (streamDto.UsernameBlack.Equals(myName))
            {
                OnWhiteSide = false;
            }

            if (streamDto.UsernameWhite.Equals(myName))
            {
                OnWhiteSide = true;
            }

            GameFirstJoined?.Invoke();
            StateReceived?.Invoke(streamDto);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to get current game state", e);
        }
    }

    public void LeaveRoomAsync()
    {
        _gameHub.LeaveRoomAsync(GameRoomId);
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

    private async void RematchOffer(GameEventDto dto)
    {
        var user = await _authService.GetAuthAsync();

        if (IsRematchOfferRequestPending)
        {
            await SendRematchResponseAsync(true);
            return;
        }

        if (dto.UsernameWhite.Equals(user.Identity!.Name) && OnWhiteSide ||
            dto.UsernameBlack.Equals(user.Identity!.Name) && !OnWhiteSide)
        {
            IsRematchOfferResponsePending = true;
        }
        else
        {
            IsRematchOfferRequestPending = true;
        }

        RematchOffered?.Invoke(dto);
    }

    private void RematchOfferTimeout(GameEventDto dto)
    {
        IsRematchOfferRequestPending = false;
        IsRematchOfferResponsePending = false;
        RematchOfferTimedOut?.Invoke(dto);
    }

    private void RematchOfferAcceptation(GameEventDto dto)
    {
        IsRematchOfferRequestPending = false;
        IsRematchOfferResponsePending = false;
        RematchOfferAccepted?.Invoke(dto);
    }

    private async void JoinRematchGame(GameEventDto dto)
    {
        if (Spectating) return;
        await JoinGameAsync(new RequestJoinGameDto()
        {
            GameRoom = dto.GameRoomId
        });

        JoinRematchedGame?.Invoke(dto);
    }

    public async Task<AckTypes> MakeMoveAsync(Move move)
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
            MoveType = (uint)move.MoveType(),
            Promotion = (uint)move.PromotedPieceType().AsInt(),
            Username = user.Identity!.Name!
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/moves", dto);
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to make a move.", e);
        }
    }

    public async Task<AckTypes> OfferDrawAsync()
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

        try
        {
            var response = await _client.PostAsJsonAsync("/draw-offers", dto);
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to offer a draw.", e);
        }
    }

    public async Task<AckTypes> SendDrawResponseAsync(bool accepted)
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
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to respond to a draw offer.", e);
        }
    }

    public async Task<AckTypes> OfferRematchAsync()
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var dto = new RequestRematchDto()
        {
            Username = user.Identity!.Name!,
            GameRoom = GameRoomId.Value
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/rematch-offers", dto);
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to offer a draw.", e);
        }
    }

    public async Task<AckTypes> SendRematchResponseAsync(bool accepted)
    {
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity != null;

        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (!isLoggedIn)
            throw new InvalidOperationException("User not logged in.");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var dto = new ResponseRematchDto
        {
            GameRoom = GameRoomId.Value,
            Accept = accepted
        };

        try
        {
            var response = await _client.PostAsJsonAsync("/rematch-responses", dto);
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to respond to a draw offer.", e);
        }
    }

    public async Task<AckTypes> ResignAsync()
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

        try
        {
            var response = await _client.PostAsJsonAsync("/resignation", dto);
            return await ResponseParser.ParseAsync<AckTypes>(response);
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to resign from the game.", e);
        }
    }

    public async Task<IList<GameRoomDto>> GetGameRoomsAsync(GameRoomSearchParameters parameters)
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
            var roomList = await ResponseParser.ParseAsync<IEnumerable<GameRoomDto>>(response);
            return roomList.ToList();
        }
        catch (HttpRequestException e)
        {
            throw new HttpRequestException("Network error. Failed to retrieve game rooms.", e);
        }
    }
}
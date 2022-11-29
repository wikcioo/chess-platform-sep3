using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Application.LogicInterfaces;
using Application.Signalr;
using Domain.DTOs;
using Domain.DTOs.GameRoomData;
using Domain.Enums;
using HttpClients;
using HttpClients.ClientInterfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Rudzoft.ChessLib.Enums;
using Rudzoft.ChessLib.Types;

namespace Application.LogicImplementations;

public class GameLogic : IGameLogic
{
    private readonly IAuthService _authService;
    public bool IsDrawOfferPending { get; set; } = false;
    public bool OnWhiteSide { get; set; } = true;
    public ulong? GameRoomId { get; set; }


    //Todo Possibility of replacing StreamUpdate with action and only needed information instead of dto
    public delegate void StreamUpdate(JoinedGameStreamDto dto);

    public event StreamUpdate? TimeUpdated;
    public event StreamUpdate? NewFenReceived;
    public event StreamUpdate? ResignationReceived;
    public event StreamUpdate? InitialTimeReceived;
    public event StreamUpdate? DrawOffered;
    public event StreamUpdate? DrawOfferTimedOut;
    public event StreamUpdate? DrawOfferAccepted;
    public event StreamUpdate? EndOfTheGameReached;
    public event Action? GameFirstJoined;

    public event Action<CurrentGameStateDto>? StateReceived;

    //Signalr
    private HubConnectionDto _hubDto;
    private HttpClient _client;

    public GameLogic(IAuthService authService, HubConnectionDto hubDto, HttpClient client)
    {
        _authService = authService;
        _hubDto = hubDto;
        _client = client;
        _hubDto.HubConnection?.On<JoinedGameStreamDto>("GameStreamDto",
            ListenToJoinedGameStream);
    }

    public GameLogic(IAuthService authService)
    {
        _authService = authService;
    }

    public async void LeaveRoom()
    {
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.SendAsync("LeaveRoom", GameRoomId);
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
        var response = await _client.PostAsJsonAsync("/startGame", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
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
        if (_hubDto.HubConnection == null)
        {
            throw new InvalidOperationException("You are not logged in!");
        }

        await _hubDto.HubConnection.SendAsync("JoinGame", dto);
        GameRoomId = dto.GameRoom;
        if (_hubDto.HubConnection is not null)
        {
            await _hubDto.HubConnection.SendAsync("JoinRoom", GameRoomId);
        }
    }


    private void ListenToJoinedGameStream(JoinedGameStreamDto response)
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
            case GameStreamEvents.InitialTime:
                InitialTime(response);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void TimeUpdate(JoinedGameStreamDto dto)
    {
        TimeUpdated?.Invoke(dto);
    }

    public async void GetCurrentGameState()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var user = await _authService.GetAuthAsync();
        var isLoggedIn = user.Identity;

        if (isLoggedIn == null || isLoggedIn.IsAuthenticated == false)
            throw new InvalidOperationException("User not logged in.");
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        var response = await _client.PostAsJsonAsync("/gameState", GameRoomId.Value);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        var streamDto = JsonSerializer.Deserialize<CurrentGameStateDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;


        var myName = user.Identity!.Name;
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

    private async void InitialTime(JoinedGameStreamDto dto)
    {
        var user = await _authService.GetAuthAsync();
        var myName = user.Identity!.Name;
        if (dto.UsernameBlack.Equals(myName))
        {
            OnWhiteSide = false;
        }

        if (dto.UsernameWhite.Equals(myName))
        {
            OnWhiteSide = true;
        }

        GameFirstJoined?.Invoke();
        InitialTimeReceived?.Invoke(dto);
    }

    private async void DrawOffer(JoinedGameStreamDto dto)
    {
        var user = await _authService.GetAuthAsync();
        if (dto.UsernameWhite.Equals(user.Identity!.Name) || dto.UsernameBlack.Equals(user.Identity!.Name))
            IsDrawOfferPending = true;

        DrawOffered?.Invoke(dto);
    }

    private void DrawOfferTimeout(JoinedGameStreamDto dto)
    {
        IsDrawOfferPending = false;
        DrawOfferTimedOut?.Invoke(dto);
    }

    private void DrawOfferAcceptation(JoinedGameStreamDto dto)
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
            MoveType = (uint) move.MoveType(),
            Promotion = (uint) move.PromotedPieceType().AsInt(),
            Username = user.Identity!.Name!
        };
        var response = await _client.PostAsJsonAsync("/makeMove", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
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
            throw new InvalidOperationException("User not logged in");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var dto = new RequestDrawDto()
        {
            Username = user.Identity!.Name!,
            GameRoom = GameRoomId.Value
        };
        var response = await _client.PostAsJsonAsync("/offerDraw", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public async Task Resign()
    {
        if (!GameRoomId.HasValue)
            throw new InvalidOperationException("You didn't join a game room!");

        if (_hubDto.HubConnection == null)
            throw new InvalidOperationException("User not logged in.");
        await _hubDto.HubConnection.SendAsync("Resign", new RequestResignDto()
        {
            GameRoom = GameRoomId.Value
        });
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
            Username = user.Identity!.Name, GameRoom = GameRoomId.Value, Accept = accepted
        };
        var response = await _client.PostAsJsonAsync("/drawResponse", dto);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        var ack = JsonSerializer.Deserialize<AckTypes>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return ack;
    }

    public async Task<IList<SpectateableGameRoomDataDto>> GetAllSpectateableGames()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());

        var response = await _client.GetAsync("/spectateable");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        var roomList = JsonSerializer.Deserialize<IEnumerable<SpectateableGameRoomDataDto>>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return roomList.ToList();
    }

    public async Task<IList<JoinableGameRoomDataDto>> GetAllJoinableGames()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authService.GetJwtToken());
        var response = await _client.GetAsync("/joinable");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseContent);
        }

        var roomList = JsonSerializer.Deserialize<IEnumerable<JoinableGameRoomDataDto>>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return roomList.ToList();
    }
}
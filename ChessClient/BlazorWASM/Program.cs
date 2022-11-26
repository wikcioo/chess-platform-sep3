using Application.LogicImplementations;
using Application.LogicInterfaces;
using Application.Signalr;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWASM;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using HttpClients.ClientInterfaces;
using HttpClients.Implementations;
using MudBlazor.Services;
using BlazorWASM.Auth;
using Domain.Auth;
using Domain.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(
    _ =>
        new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7233")
        }
);

builder.Services.AddSingleton(_ => GrpcChannel.ForAddress("http://localhost:5231", new GrpcChannelOptions
{
    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
}));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 6000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled; 
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
AuthorizationPolicies.AddPolicies(builder.Services);
builder.Services.AddScoped<IUserService, UserHttpClient>();
builder.Services.AddSingleton<HubConnectionDto>();
// builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<IGameLogic, GameLogic>();
builder.Services.AddScoped<IChatLogic, ChatLogic>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
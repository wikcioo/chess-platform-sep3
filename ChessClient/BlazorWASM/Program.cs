using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWASM;
using HttpClients.ClientInterfaces;
using HttpClients.Implementations;
using MudBlazor.Services;
using BlazorWASM.Auth;
using Domain.Auth;
using HttpClients.ClientInterfaces.Signalr;
using HttpClients.Implementations.Signalr;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(
    _ =>
        new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7233")
        }
);

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
builder.Services.AddSingleton<IUserService, UserHttpClient>();
builder.Services.AddSingleton<IAuthService, JwtAuthService>();
builder.Services.AddSingleton<IGameService, GameService>();

builder.Services.AddSingleton<IHubConnectionHandler, HubConnectionHandler>();
builder.Services.AddTransient<IChatHub, ChatHub>();
builder.Services.AddSingleton<IGameHub, GameHub>();

builder.Services.AddSingleton<AuthenticationStateProvider, CustomAuthProvider>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
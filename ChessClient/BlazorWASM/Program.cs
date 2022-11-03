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
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(
    sp =>
        new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7233")
        }
);

builder.Services.AddSingleton(services => GrpcChannel.ForAddress("http://localhost:5231", new GrpcChannelOptions
{
    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
}));

builder.Services.AddMudServices();
AuthorizationPolicies.AddPolicies(builder.Services);
builder.Services.AddScoped<IUserService, UserHttpClient>();
// builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthProvider>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
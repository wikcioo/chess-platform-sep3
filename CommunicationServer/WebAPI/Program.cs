using System.Text;
using Application.ClientInterfaces;
using Application.GameRoomHandlers;
using Application.Logic;
using Application.LogicInterfaces;
using DatabaseClient.Implementations;
using Domain.Auth;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using WebAPI;
using WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthLogic, AuthLogic>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] {"application/octet-stream"});
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddSignalR();
builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddHttpClient<IUserService, UserHttpClient>(client =>
    client.BaseAddress = new Uri("http://localhost:8080"));

builder.Services.AddHttpClient<IGameService, GameHttpClient>(client =>
    client.BaseAddress = new Uri("http://localhost:8080"));

builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

builder.Services.AddSingleton<IStockfishService, StockfishHttpClient>();

builder.Services.AddSingleton(_ => GrpcChannel.ForAddress("https://localhost:7007"));

builder.Services.AddSingleton<IGameRoomHandlerFactory, GameRoomHandlerFactory>();
builder.Services.AddSingleton<IChatLogic, ChatLogic>();
builder.Services.AddSingleton<IGameLogic, GameLogic>();

builder.Services.AddSingleton<GroupHandler>();


AuthorizationPolicies.AddPolicies(builder.Services);
var app = builder.Build();
app.UseResponseCompression();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");
app.Run();
using Application.Logic;
using Application.LogicInterfaces;
using GrpcService.Services;
using GrpcService.Services.ChessGame;
using GrpcService.Services.GameChat;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));
builder.Services.AddSingleton(new ChatRoomService());
IGameLogic gameLogic = new GameLogic();
builder.Services.AddSingleton(gameLogic);
var app = builder.Build();
app.UseResponseCompression();


app.UseStaticFiles();

app.UseRouting();

app.UseGrpcWeb();
app.UseCors();
app.MapGrpcService<ChatService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGrpcService<GameService>().EnableGrpcWeb().RequireCors("AllowAll");

app.Run();
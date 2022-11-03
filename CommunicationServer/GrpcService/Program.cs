using System.Text;
using Application.Logic;
using Application.LogicInterfaces;
using Domain.Auth;
using GrpcService.Services;
using GrpcService.Services.ChessGame;
using GrpcService.Services.GameChat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] {"application/octet-stream"});
});
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding", "Authorization");
}));

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
builder.Services.AddAuthorization();
AuthorizationPolicies.AddPolicies(builder.Services);
builder.Services.AddSingleton(new ChatRoomService());
IGameLogic gameLogic = new GameLogic();
builder.Services.AddSingleton(gameLogic);

var app = builder.Build();
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseGrpcWeb();
app.UseCors();

app.MapGrpcService<ChatService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGrpcService<GameService>().EnableGrpcWeb().RequireCors("AllowAll");

app.Run();
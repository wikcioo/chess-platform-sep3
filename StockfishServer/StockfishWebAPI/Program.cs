using Application.Logic;
using Application.LogicInterfaces;
using Microsoft.AspNetCore.ResponseCompression;
using StockfishWebAPI.Controllers;
using StockfishWrapper;


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
        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));
builder.Services.AddScoped<IStockfishLogic, StockfishLogic>();
builder.Services.AddScoped<IStockfishUci, StockfishUciImpl>();

var app = builder.Build();
app.UseResponseCompression();

app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.MapGrpcService<StockfishController>().RequireCors("AllowAll");
app.Run();
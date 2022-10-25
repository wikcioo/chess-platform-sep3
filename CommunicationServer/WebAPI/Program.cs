using Application.ClientInterfaces;
using Application.DaoInterfaces;
using Application.Logic;
using Application.LogicInterfaces;
using DatabaseClient.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserLogic, UserLogic>();
builder.Services.AddScoped<IUserService, UserHttpClient>();

builder.Services.AddScoped<IStockfishLogic, StockfishLogic>();
builder.Services.AddScoped<IStockfishService, StockfishHttpClient>();

builder.Services.AddScoped(
    sp => 
        new HttpClient { 
            BaseAddress = new Uri("http://localhost:8080") 
        }
);

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
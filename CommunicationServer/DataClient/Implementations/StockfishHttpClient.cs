using Application.ClientInterfaces;
using Domain.DTOs;
using Grpc.Core;
using Grpc.Net.Client;
using StockfishWebAPI;

namespace DatabaseClient.Implementations;

public class StockfishHttpClient : IStockfishService
{
    private readonly Stockfish.StockfishClient _client;
    private Empty _empty = new();

    public StockfishHttpClient(GrpcChannel channel)
    {
        _client = new Stockfish.StockfishClient(channel);
    }

    public async Task<bool> GetStockfishIsReadyAsync()
    {
        try
        {
            var response = await _client.GetStockfishReadyAsync(_empty);
            return response.Ready;
        }
        catch (RpcException e)
        {
            throw new HttpRequestException("Failed to connect to server", e);
        }
    }

    public async Task<string> GetBestMoveAsync(StockfishBestMoveDto dto)
    {
        try
        {
            var response = await _client.GetBestMoveAsync(new RequestBestMove
            {
                Fen = dto.Fen,
                StockfishPlayer = dto.StockfishPlayer
            });
            return response.Fen;
        }
        catch (RpcException e)
        {
            throw new HttpRequestException("Failed to connect to server", e);
        }
    }
}
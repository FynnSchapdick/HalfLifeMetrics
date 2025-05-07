using HalfLifeMetrics.Server.Options;
using Microsoft.Extensions.Options;
using RconSharp;

namespace HalfLifeMetrics.Server.Worker;

public sealed class RconWorker(IOptions<RconOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        foreach (AllowedOrigin allowedOrigin in options.Value.AllowedOrigins)
        {
            RconClient? client = RconClient.Create(allowedOrigin.IpEndPoint.Address.ToString(), allowedOrigin.IpEndPoint.Port);
        
            await client.ConnectAsync();
        
            bool authenticated = await client.AuthenticateAsync(allowedOrigin.Password);
            if (authenticated)
            {
                string? status = await client.ExecuteCommandAsync("status");
                
                Console.WriteLine(status);
            }
        }
    }
}
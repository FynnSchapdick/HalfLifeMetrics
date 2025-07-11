﻿using System.Net.Sockets;
using System.Text;
using HalfLifeMetrics.Server.MessageHandlers;
using HalfLifeMetrics.Server.Options;
using Microsoft.Extensions.Options;

namespace HalfLifeMetrics.Server.Worker;

public sealed class RconListener(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RconOptions> options,
    ILogger<RconListener> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        using UdpClient client = new(options.Value.ReceivePort);
        logger.LogInformation("RconListener started on port {Port}", options.Value.ReceivePort);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await client.ReceiveAsync(stoppingToken);
                string message = Encoding.UTF8.GetString(result.Buffer);
                _ = Task.Run(() => OnMessageReceived(message, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("RconListener stopped");
        }
    }

    private async Task OnMessageReceived(string message, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        IRconMessageHandler[] rconMessageHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IRconMessageHandler>>().ToArray();
        await Task.WhenAll(rconMessageHandlers.Select(x => x.HandleMessage(message, cancellationToken)));
    }
}

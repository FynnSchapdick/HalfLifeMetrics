using System.Threading.Channels;
using HalfLifeMetrics.Data;
using HalfLifeMetrics.Server.Data;
using HalfLifeMetrics.Server.Data.Entities;
using HalfLifeMetrics.Server.MessageHandlers;
using Microsoft.EntityFrameworkCore;

namespace HalfLifeMetrics.Server.Worker;

public sealed class PlayerConnectedWorker(Channel<PlayerConnected> channel, IDbContextFactory<HalfLifeStatsDbContext> contextFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        await foreach (PlayerConnected playerConnected in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await using HalfLifeStatsDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);

            await context.Sessions.AddAsync(new SessionEntity
            {
                SteamProfileUrl = playerConnected.SteamId.ToSteamProfileUrl(),
                Nickname = playerConnected.Name,
                IpAddress = playerConnected.Ip.ToString()
            }, stoppingToken);

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
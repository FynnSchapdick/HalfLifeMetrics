using System.Threading.Channels;
using HalfLifeMetrics.Server.Data;
using HalfLifeMetrics.Server.Data.Entities;
using HalfLifeMetrics.Server.MessageHandlers;
using Microsoft.EntityFrameworkCore;

namespace HalfLifeMetrics.Server.Worker;

public sealed class PlayerDisconnectedWorker(Channel<PlayerDisconnected> channel, IDbContextFactory<HalfLifeStatsDbContext> contextFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        await foreach (PlayerDisconnected playerDisconnected in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await using HalfLifeStatsDbContext context = await contextFactory.CreateDbContextAsync(stoppingToken);
            SessionEntity? session = await context.Sessions
                .AsTracking()
                .Where(x => x.ClosedAt == null)
                .Where(x => x.SteamProfileUrl == playerDisconnected.SteamId.ToSteamProfileUrl())
                .Where(x => x.Nickname == playerDisconnected.Name)
                .SingleOrDefaultAsync(stoppingToken);
            
            if (session is null)
            {
                return;
            }
            
            session.ClosedAt = DateTime.UtcNow;
            if (Enum.TryParse(playerDisconnected.Reason, out CloseReason closeReason))
            {
                session.CloseReason = closeReason;
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
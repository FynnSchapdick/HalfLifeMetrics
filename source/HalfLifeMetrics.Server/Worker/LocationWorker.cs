using System.Net;
using System.Text.Json;
using HalfLifeMetrics.Server.Apis;
using HalfLifeMetrics.Server.Data;
using HalfLifeMetrics.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Refit;

namespace HalfLifeMetrics.Server.Worker;

public sealed class LocationWorker(
    IIpApi ipApi,
    IDbContextFactory<HalfLifeStatsDbContext> dbContextFactory,
    ILogger<LocationWorker> logger) : BackgroundService
{
    private int TimeUntilRefresh { get; set; }
    private int Remaining { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateLocationsAsync(stoppingToken);
        }
    }

    private async Task UpdateLocationsAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using HalfLifeStatsDbContext context = await dbContextFactory.CreateDbContextAsync(stoppingToken);
            SessionEntity[] sessions = await context.Sessions
                .AsTracking()
                .Where(x => x.GeoIpLocation == null)
                .ToArrayAsync(stoppingToken);

            foreach (SessionEntity[] session in sessions.Chunk(100))
            {
                if (Remaining <= 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(TimeUntilRefresh), stoppingToken);
                }

                ApiResponse<IpInfoDto[]> response = await ipApi.GetIpInfoAsync(session
                    .Select(x => x.IpAddress)
                    .Distinct()
                    .Select(x => x)
                    .ToArray(), stoppingToken);

                TimeUntilRefresh = response.Headers.GetValues("X-Ttl").Select(int.Parse).FirstOrDefault();
                Remaining = response.Headers.GetValues("X-Rl").Select(int.Parse).FirstOrDefault();

                if (response.StatusCode == HttpStatusCode.TooManyRequests) break;

                if (!response.IsSuccessStatusCode || response.Content is not { } ipInfoDto)
                {
                    using IDisposable? _ = logger.BeginScope(new Dictionary<string, object>
                    {
                        ["Request"] = session,
                        ["Response"] = response
                    });

                    logger.LogError("Failed to get location data");
                    break;
                }

                foreach (IpInfoDto infoDto in ipInfoDto.DistinctBy(x => x.Query))
                {
                    GeoIpLocationEntity locationEntity = new()
                    {
                        Latitude = infoDto.Lat,
                        Longitude = infoDto.Lon,
                        Metadata = JsonSerializer.SerializeToDocument(infoDto)
                    };

                    sessions.Where(x => x.IpAddress == infoDto.Query)
                        .ToList()
                        .ForEach(x => x.GeoIpLocation = locationEntity);
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Fehler beim Abrufen von IP-Daten");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating locations");
        }
    }
}
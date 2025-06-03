using System.Text.RegularExpressions;
using HalfLifeMetrics.Server.Metric;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class MapLoadedHandler(
    ILogger<MapLoadedHandler> logger,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex(@"Loading map \""(?<MapName>[^\""]+)\""", RegexOptions.Compiled)]
    public partial Regex MapLoadedRegex { get; }
    
    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!MapLoadedRegex.TryMatchMapLoaded(message, out MapLoaded? mapLoaded) || mapLoaded is null)
            {
                return Task.CompletedTask;
            }
            
            metricService.MapsLoaded
                .WithLabels(mapLoaded.MapName)
                .Inc();
            
            logger.LogInformation("Map loaded: {MapName}", mapLoaded.MapName);
        }
        catch (Exception e)
        {
           logger.LogError(e, "An error occured while loading map");
        }
        
        return Task.CompletedTask;
    }
}

public sealed record MapLoaded(string MapName);

file static class Extensions
{
    public static bool TryMatchMapLoaded(this Regex regex, string message, out MapLoaded? mapLoaded)
    {
        mapLoaded = null;
        if (regex.Match(message) is not { Success: true } match)
        {
            return false;
        }
        
        mapLoaded = match.ToMapLoaded();
        return true;
    }

    private static MapLoaded ToMapLoaded(this Match match) => new(match.Groups["MapName"].Value);
}
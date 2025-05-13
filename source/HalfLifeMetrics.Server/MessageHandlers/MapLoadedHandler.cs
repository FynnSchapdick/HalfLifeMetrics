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
            if (!message.Contains("Loading map"))
            {
                return Task.CompletedTask;
            }
            
            if (MapLoadedRegex.Match(message) is not { Success: true } match)
            {
                return Task.CompletedTask;
            }
        
            string mapName = match.Groups["MapName"].Value;
            metricService.MapsLoaded
                .WithLabels(mapName)
                .Inc();
            
            logger.LogInformation("Map loaded: {MapName}", mapName);
        }
        catch (Exception e)
        {
           logger.LogError(e, "An error occured while loading map");
        }
        
        return Task.CompletedTask;
    }
}
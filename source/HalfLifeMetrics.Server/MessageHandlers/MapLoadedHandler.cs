using System.Text.RegularExpressions;
using Prometheus;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class MapLoadedHandler(ILogger<MapLoadedHandler> logger) : IRconMessageHandler
{
    [GeneratedRegex(@"Loading map \""(?<MapName>[^\""]+)\""", RegexOptions.Compiled)]
    public partial Regex MapLoadedRegex { get; }
    
    private readonly Counter _mapLoadedCounter = Metrics.CreateCounter(
        "map_loaded_total",
        "Counts the number of maps been loaded",
        new CounterConfiguration { LabelNames = ["map_name"]}
    );

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
            _mapLoadedCounter
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
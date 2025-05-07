using Prometheus;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed class PlayerDisconnectedHandler(
    ILogger<PlayerDisconnectedHandler> logger)
    : IRconMessageHandler
{
    public readonly Counter DisconnectedPlayers = Metrics.CreateCounter(
        "disconnected_players_total",
        "Counts the number of disconnected players"
        , new CounterConfiguration
        {
            LabelNames = ["steam_name", "ip_address"]
        });
    
    
    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!message.Contains("disconnected"))
            {
                return Task.CompletedTask;
            }
            
            DisconnectedPlayers.Inc();
            
            logger.LogInformation("Player disconnected: {Message}", message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerDisconnected message");
        }
        
        return Task.CompletedTask;
    }
}
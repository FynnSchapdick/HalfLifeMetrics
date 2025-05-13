using System.Text.RegularExpressions;
using System.Threading.Channels;
using HalfLifeMetrics.Server.Metric;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerDisconnectedHandler(
    ILogger<PlayerDisconnectedHandler> logger,
    Channel<PlayerDisconnected> channel,
    MetricService metricService)
    : IRconMessageHandler
{
    
    [GeneratedRegex("\"(?<Name>.+?)<(?<ClientID>\\d+)>\\<(?<SteamID>[^>]+)>\\<(?<Team>[^>]+)>\" disconnected \\(reason \"(?<Reason>.*?)\"\\)", RegexOptions.Compiled)]
    public partial Regex PlayerDisconnectedRegex { get; }
    
    public async Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!message.Contains("disconnected"))
            {
                return;
            }
            
            if (PlayerDisconnectedRegex.Match(message) is not {Success: true} match)
            {
                logger.LogWarning("PlayerDisconnectedRegex did not match message: {Message}", message);
                return;
            }

            if (metricService.CurrentPlayers.Value > 0)
            {
                metricService.CurrentPlayers.Dec();
            }

            PlayerDisconnected playerDisconnected = match.ToPlayerDisconnected();
            
            await channel.Writer.WriteAsync(playerDisconnected, cancellationToken);
            
            logger.LogInformation("Player {Name} disconnected with SteamID {SteamId} with reason {Reason}", playerDisconnected.Name, playerDisconnected.SteamId.ToSteamId3(), playerDisconnected.Reason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerDisconnected message");
        }
    }
}

public sealed record PlayerDisconnected(string Name, SteamIdBase SteamId, string Reason);

file static class Extensions
{
    public static PlayerDisconnected ToPlayerDisconnected(this Match match) => new
    (
        match.Groups["Name"].Value,
        SteamId3.Parse(match.Groups["SteamID"].Value),
        match.Groups["Reason"].Value
    );
}
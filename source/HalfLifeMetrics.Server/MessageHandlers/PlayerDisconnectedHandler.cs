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
    
    [GeneratedRegex("\"(?<PlayerName>.+?)<(?<ClientID>\\d+)>\\<(?<PlayerSteamID3>[^>]+)>\\<(?<Team>[^>]+)>\" disconnected \\(reason \"(?<Reason>.*?)\"\\)", RegexOptions.Compiled)]
    public partial Regex PlayerDisconnectedRegex { get; }
    
    public async Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!PlayerDisconnectedRegex.TryMatchPlayerDisconnected(message, out PlayerDisconnected? playerDisconnected) || playerDisconnected is null)
            {
                return;
            }

            if (metricService.CurrentPlayers.Value > 0)
            {
                metricService.CurrentPlayers.Dec();
            }

            await channel.Writer.WriteAsync(playerDisconnected, cancellationToken);

            logger.LogInformation("Player '{Name}' disconnected with SteamID3 '{SteamId3}' with reason '{Reason}'", playerDisconnected.Name, playerDisconnected.SteamId.ToSteamId3(), playerDisconnected.Reason);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("PlayerDisconnectedHandler operation was canceled.");
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
    public static bool TryMatchPlayerDisconnected(this Regex regex, string message, out PlayerDisconnected? playerDisconnected)
    {
        playerDisconnected = null;
        if (regex.Match(message) is not {Success: true} match)
        {
            return false;
        }
        
        playerDisconnected = match.ToPlayerDisconnected();
        return true;
    }
    
    private static PlayerDisconnected ToPlayerDisconnected(this Match match) => new
    (
        match.Groups["PlayerName"].Value,
        SteamId3.Parse(match.Groups["PlayerSteamID3"].Value),
        match.Groups["Reason"].Value
    );
}
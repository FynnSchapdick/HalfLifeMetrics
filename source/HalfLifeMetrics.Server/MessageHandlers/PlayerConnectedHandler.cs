using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using HalfLifeMetrics.Server.Metric;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerConnectedHandler(
    ILogger<PlayerConnectedHandler> logger,
    Channel<PlayerConnected> channel,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex("(\"(?<PlayerName>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<PlayerSteamID3>.+?)><(?<Team>.+?)?>\") connected, address \"(?<Host>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerConnectedRegex { get; }
    

    public async Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!PlayerConnectedRegex.TryMatchPlayerConnected(message, out PlayerConnected? playerConnected) || playerConnected is null)
            {
                return;
            }

            metricService.CurrentPlayers.Inc();
            
            await channel.Writer.WriteAsync(playerConnected, cancellationToken);
                
            logger.LogInformation("Player '{Name}' connected with SteamID3 '{SteamId3}' from IP '{Ip}'", playerConnected.Name, playerConnected.SteamId.ToSteamId3(), playerConnected.Ip.ToString());
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("PlayerConnectedHandler operation was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in PlayerConnectedHandler");
        }
    }
}

public sealed record PlayerConnected(string Name, SteamIdBase SteamId, IPAddress Ip);

file static class Extensions
{
    public static bool TryMatchPlayerConnected(this Regex regex, string message, out PlayerConnected? playerConnected)
    {
        playerConnected = null;
        if (regex.Match(message) is not {Success: true} match)
        {
            return false;
        }
        
        playerConnected = match.ToPlayerConnected();
        return true;
    }

    private static PlayerConnected ToPlayerConnected(this Match match) => new
    (
        match.Groups["PlayerName"].Value,
        SteamId3.Parse(match.Groups["PlayerSteamID3"].Value),
        IPAddress.Parse(match.Groups["Host"].Value.Split(':')[0])
    );
}
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
    [GeneratedRegex("(\"(?<Name>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<SteamID>.+?)><(?<Team>.+?)?>\") connected, address \"(?<Host>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerConnectedRegex { get; }
    

    public async Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!message.Contains("connected"))
            {
                return;
            }

            if (PlayerConnectedRegex.Match(message) is not {Success: true} match)
            {
                logger.LogWarning("PlayerConnectedRegex did not match message: {Message}", message);
                return;
            }
            
            metricService.CurrentPlayers.Inc();

            PlayerConnected playerConnected = match.ToPlayerConnected();
            
            await channel.Writer.WriteAsync(playerConnected, cancellationToken);
            
            logger.LogInformation("Player {Name} connected with SteamID {SteamId} from IP {Ip}", playerConnected.Name, playerConnected.SteamId.ToSteamId3(), playerConnected.Ip.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerConnected message");
        }
    }
}

public sealed record PlayerConnected(string Name, SteamIdBase SteamId, IPAddress Ip);

file static class Extensions
{
    public static PlayerConnected ToPlayerConnected(this Match match) => new
    (
        match.Groups["Name"].Value,
        SteamId3.Parse(match.Groups["SteamID"].Value),
        IPAddress.Parse(match.Groups["Host"].Value.Split(':')[0])
    );
}
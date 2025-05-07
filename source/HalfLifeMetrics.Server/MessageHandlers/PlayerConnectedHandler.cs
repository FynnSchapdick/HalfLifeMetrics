using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Prometheus;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerConnectedHandler(
    ILogger<PlayerConnectedHandler> logger,
    Channel<PlayerConnected> channel) : IRconMessageHandler
{
    [GeneratedRegex("(\"(?<Name>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<SteamID>.+?)><(?<Team>.+?)?>\") connected, address \"(?<Host>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerConnectedRegex { get; }
    
    public readonly Counter ConnectedPlayers = Metrics.CreateCounter(
        "connected_players_total",
        "Counts the number of connected players");

    public async Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Contains("connected") is false)
            {
                return;
            }

            if (PlayerConnectedRegex.Match(message) is not {Success: true} match)
            {
                logger.LogWarning("PlayerConnectedRegex did not match message: {Message}", message);
                return;
            }
            
            string name = match.Groups["Name"].Value;
            SteamIdBase steamId = SteamIdBase.Parse(match.Groups["SteamID"].Value);
            string ip = match.Groups["Host"].Value.Split(':')[0];

            await channel.Writer.WriteAsync(
                new PlayerConnected(
                    name,
                    steamId,
                    IPAddress.Parse(ip))
                , cancellationToken);

            ConnectedPlayers.Inc();
            
            logger.LogInformation("Player {Name} connected with SteamID {SteamId} from IP {Ip}", name, steamId, ip);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerConnected message");
        }
    }
}

public sealed record PlayerConnected(string Name, SteamIdBase SteamId, IPAddress Ip);
using System.Text.RegularExpressions;
using HalfLifeMetrics.Server.Metric;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerCommitedSuicideHandler(
    ILogger<PlayerCommitedSuicideHandler> logger,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex("(\"(?<Name>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<SteamID>.+?)><(?<Team>.+?)?>\") committed suicide with \"(?<Weapon>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerSuicideRegex { get; }

    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (PlayerSuicideRegex.Match(message) is not { Success: true } match)
            {
                return Task.CompletedTask;
            }

            string steamId64 =  SteamId3.Parse(match.Groups["SteamID"].Value).ToSteamId64().ToString();
            string name = match.Groups["Name"].Value;
            string weapon = match.Groups["Weapon"].Value;
            
            metricService.CommitedSuicides
                .WithLabels(steamId64, name, weapon)
                .Inc();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Player Commited Suicide message");
        }
        
        return Task.CompletedTask;
    }
}
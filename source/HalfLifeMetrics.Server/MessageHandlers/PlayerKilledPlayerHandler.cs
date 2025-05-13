using System.Text.RegularExpressions;
using HalfLifeMetrics.Server.Metric;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerKilledPlayerHandler(
    ILogger<PlayerKilledPlayerHandler> logger,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex(@"""(?<AttackerName>[^<]+)<(?<AttackerId>\d+)><(?<AttackerSteamId>\[U:\d+:\d+\])><(?<AttackerTeam>[^>]+)>""\s+killed\s+""(?<VictimName>[^<]+)<(?<VictimId>\d+)><(?<VictimSteamId>\[U:\d+:\d+\])><(?<VictimTeam>[^>]+)>""\s+with\s+""(?<Weapon>[^""]+)""", RegexOptions.Compiled)]
    private static partial Regex PlayerKilledPlayer { get; }
    
    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!message.Contains("killed"))
            {
                return Task.CompletedTask;
            }
            
            if (PlayerKilledPlayer.Match(message) is not { Success: true } match)
            {
                return Task.CompletedTask;
            }
            
            string attackerName = match.Groups["AttackerName"].Value;
            string attackerSteamId = match.Groups["AttackerSteamId"].Value;
            string victimName = match.Groups["VictimName"].Value;
            string victimSteamId = match.Groups["VictimSteamId"].Value;
            string weapon = match.Groups["Weapon"].Value;
            
            metricService.PlayerKills
                .WithLabels(attackerSteamId, weapon, attackerName)
                .Inc();
            
            metricService.PlayerDeaths
                .WithLabels(victimSteamId, weapon, victimName)
                .Inc();
            
            logger.LogInformation("Player {AttackerName} killed {VictimName} with {Weapon}", attackerName, victimName, weapon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerKilledPlayer message");
        }

        return Task.CompletedTask;
    }
}
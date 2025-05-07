using System.Text.RegularExpressions;
using Prometheus;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerKilledPlayerHandler(ILogger<PlayerKilledPlayerHandler> logger) : IRconMessageHandler
{
    [GeneratedRegex(@"""(?<AttackerName>[^<]+)<(?<AttackerId>\d+)><(?<AttackerSteamId>\[U:\d+:\d+\])><(?<AttackerTeam>[^>]+)>""\s+killed\s+""(?<VictimName>[^<]+)<(?<VictimId>\d+)><(?<VictimSteamId>\[U:\d+:\d+\])><(?<VictimTeam>[^>]+)>""\s+with\s+""(?<Weapon>[^""]+)""", RegexOptions.Compiled)]
    private static partial Regex PlayerKilledPlayer { get; }
    
    private readonly Counter _playerKillsCounter = Metrics.CreateCounter(
        "player_kills_total",
        "Counts the number of kills",
        new CounterConfiguration { LabelNames = ["steam_id", "weapon_name", "steam_name"]}
    );
    
    private readonly Counter _playerDeathsCounter = Metrics.CreateCounter(
        "player_deaths_total",
        "Counts the number of deaths",
        new CounterConfiguration { LabelNames = ["steam_id", "weapon_name", "steam_name"]}
    );
    
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
            
            _playerKillsCounter
                .WithLabels(attackerSteamId, weapon, attackerName)
                .Inc();
            
            _playerDeathsCounter
                .WithLabels(victimSteamId, weapon, victimName)
                .Inc();
            
            logger.LogInformation("Player {AttackerName} killed {VictimName} with {Weapon}", attackerName, victimName, weapon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerDisconnected message");
        }

        return Task.CompletedTask;
    }
}
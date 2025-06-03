using System.Text.RegularExpressions;
using HalfLifeMetrics.Server.Metric;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerKilledVictimHandler(
    ILogger<PlayerKilledVictimHandler> logger,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex(@"""(?<PlayerName>[^<]+)<(?<PlayerId>\d+)><(?<PlayerSteamId3>\[U:\d+:\d+\])><(?<PlayerTeam>[^>]+)>""\s+killed\s+""(?<VictimName>[^<]+)<(?<VictimId>\d+)><(?<VictimSteamId3>\[U:\d+:\d+\])><(?<VictimTeam>[^>]+)>""\s+with\s+""(?<Weapon>[^""]+)""", RegexOptions.Compiled)]
    private static partial Regex PlayerKilledPlayer { get; }
    
    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!PlayerKilledPlayer.TryMatchPlayerKilledVictim(message, out PlayerKilledVictim? playerKilledVictim) || playerKilledVictim is null)
            {
                return Task.CompletedTask;
            }
            
            metricService.PlayerKills
                .WithLabels(playerKilledVictim.PlayerSteamId.ToSteamId3(), playerKilledVictim.Weapon, playerKilledVictim.PlayerName)
                .Inc();
            
            metricService.PlayerDeaths
                .WithLabels(playerKilledVictim.VictimSteamId.ToSteamId3(), playerKilledVictim.Weapon, playerKilledVictim.VictimName)
                .Inc();
            
            logger.LogInformation("Player '{PlayerName}' killed Victim '{VictimName}' with '{Weapon}'", playerKilledVictim.PlayerName, playerKilledVictim.VictimName, playerKilledVictim.Weapon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PlayerKilledPlayer message");
        }

        return Task.CompletedTask;
    }
}

public sealed record PlayerKilledVictim(
    string PlayerName,
    SteamIdBase PlayerSteamId,
    string VictimName,
    SteamIdBase VictimSteamId,
    string Weapon);

file static class Extensions
{
    public static bool TryMatchPlayerKilledVictim(this Regex regex, string message, out PlayerKilledVictim? playerKilledVictim)
    {
        playerKilledVictim = null;
        if (regex.Match(message) is not { Success: true } match)
        {
            return false;
        }
        
        playerKilledVictim = match.ToPlayerKilledVictim();
        return true;
    }
    
    private static PlayerKilledVictim ToPlayerKilledVictim(this Match match) => new(
        match.Groups["PlayerName"].Value,
        SteamIdBase.Parse(match.Groups["PlayerSteamId3"].Value),
        match.Groups["VictimName"].Value,
        SteamIdBase.Parse(match.Groups["VictimSteamId3"].Value),
        match.Groups["Weapon"].Value);
}
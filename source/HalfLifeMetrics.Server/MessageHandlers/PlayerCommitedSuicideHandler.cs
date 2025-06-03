using System.Text.RegularExpressions;
using HalfLifeMetrics.Server.Metric;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerCommitedSuicideHandler(
    ILogger<PlayerCommitedSuicideHandler> logger,
    MetricService metricService) : IRconMessageHandler
{
    [GeneratedRegex("(\"(?<PlayerName>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<PlayerSteamID3>.+?)><(?<Team>.+?)?>\") committed suicide with \"(?<Weapon>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerCommitedSuicideRegex { get; }

    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (!PlayerCommitedSuicideRegex.TryMatchPlayerCommitedSuicide(message, out PlayerCommitedSuicide? playerCommitedSuicide) || playerCommitedSuicide is null)
            {
                return Task.CompletedTask;
            }
            
            metricService.CommitedSuicides
                .WithLabels(playerCommitedSuicide.PlayerSteamId.ToSteamId3(), playerCommitedSuicide.PlayerName, playerCommitedSuicide.Weapon)
                .Inc();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Player Commited Suicide message");
        }
        
        return Task.CompletedTask;
    }
}

public sealed record PlayerCommitedSuicide(string PlayerName, SteamIdBase PlayerSteamId, string Weapon);

file static class Extensions
{
    public static bool TryMatchPlayerCommitedSuicide(this Regex regex, string message, out PlayerCommitedSuicide? playerCommitedSuicide)
    {
        playerCommitedSuicide = null;
        if (regex.Match(message) is not {Success: true} match)
        {
            return false;
        }

        playerCommitedSuicide = match.ToPlayerCommitedSuicide();
        return true;
    }

    private static PlayerCommitedSuicide ToPlayerCommitedSuicide(this Match match) => new
    (
        match.Groups["PlayerName"].Value,
        SteamId3.Parse(match.Groups["PlayerSteamID3"].Value),
        match.Groups["Weapon"].Value
    );
}
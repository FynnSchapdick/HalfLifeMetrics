using System.Text.RegularExpressions;
using Prometheus;
using SteamId.Net;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed partial class PlayerCommitedSuicideHandler(ILogger<PlayerCommitedSuicideHandler> logger) : IRconMessageHandler
{
    [GeneratedRegex("(\"(?<Name>.+?(?:<.*>)*)<(?<ClientID>\\d+?)><(?<SteamID>.+?)><(?<Team>.+?)?>\") committed suicide with \"(?<Weapon>.+?)\"", RegexOptions.Compiled)]
    public partial Regex PlayerSuicideRegex { get; }
    
    private readonly Counter _playerCommitedSuicide = Metrics.CreateCounter(
        "player_commited_suicide_total",
        "Counts the number of players who commited suicide",
        new CounterConfiguration { LabelNames = ["steam_id64", "steam_name", "weapon"]}
    );

    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (PlayerSuicideRegex.Match(message) is not { Success: true } match)
            {
                return Task.CompletedTask;
            }

            string steamId64 = SteamIdBase.Parse(match.Groups["SteamID"].Value).ToSteamId64().ToString();
            string name = match.Groups["Name"].Value;
            string weapon = match.Groups["Weapon"].Value;
            
            _playerCommitedSuicide
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
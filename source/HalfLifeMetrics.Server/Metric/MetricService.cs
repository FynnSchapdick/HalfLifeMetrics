using Prometheus;

namespace HalfLifeMetrics.Server.Metric;

public sealed class MetricService
{
    public readonly Gauge CurrentPlayers = Metrics.CreateGauge(
        "hl_current_players", 
        "Current number of players");
    
    public readonly Counter CommitedSuicides = Metrics.CreateCounter(
        "hl_commited_suicides",
        "Counts the number of players who commited suicide",
        new CounterConfiguration { LabelNames = ["steam_id64", "steam_name", "weapon"]}
    );

    public readonly Counter MapsLoaded = Metrics.CreateCounter(
        "hl_maps_loaded",
        "Counts the number of maps been loaded",
        new CounterConfiguration { LabelNames = ["map_name"]}
    );
    
    public readonly Counter PlayerKills = Metrics.CreateCounter(
        "hl_player_kills",
        "Counts the number of kills",
        new CounterConfiguration { LabelNames = ["steam_id", "weapon_name", "steam_name"]}
    );
    
    public readonly Counter PlayerDeaths = Metrics.CreateCounter(
        "hl_player_deaths",
        "Counts the number of deaths",
        new CounterConfiguration { LabelNames = ["steam_id", "weapon_name", "steam_name"]}
    );
}
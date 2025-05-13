namespace HalfLifeMetrics.Server.Data.Entities;

public sealed class SessionEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string SteamProfileUrl { get; init; }
    public required string Nickname { get; init; }
    public required string IpAddress { get; init; }
    public GeoIpLocationEntity? GeoIpLocation { get; set; }
    public DateTimeOffset OpenedAt { get; private init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }
    public CloseReason? CloseReason { get; set; }
}

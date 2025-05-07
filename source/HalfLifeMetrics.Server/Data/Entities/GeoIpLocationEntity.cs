using System.Text.Json;

namespace HalfLifeMetrics.Server.Data.Entities;

public sealed class GeoIpLocationEntity
{
    public required JsonDocument Metadata { get; set; } = null!;
    public required double Latitude { get; set; }
    public required double Longitude  { get; set; }
}

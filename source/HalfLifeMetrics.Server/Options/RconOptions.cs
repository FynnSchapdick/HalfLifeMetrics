namespace HalfLifeMetrics.Server.Options;

public sealed class RconOptions
{
    public int ReceivePort { get; set; } = 0;
    public AllowedOrigin[] AllowedOrigins { get; set; } = [];
}
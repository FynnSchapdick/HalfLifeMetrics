using System.Net;

namespace HalfLifeMetrics.Server.Options;

public sealed class AllowedOrigin
{
    public required IPEndPoint IpEndPoint { get; set; }
    public string? Password { get; set; }

    public static AllowedOrigin Empty { get; } = new()
    {
        IpEndPoint = new IPEndPoint(IPAddress.None, 0),
    };
}
using Refit;

namespace HalfLifeMetrics.Server.Apis;

public interface IIpApi
{
    [Post("/batch")]
    Task<ApiResponse<IpInfoDto[]>> GetIpInfoAsync([Body] string[] query, CancellationToken cancellationToken = default);
}

public record IpInfoDto(
    string Status,
    string Country,
    string CountryCode,
    string Region,
    string RegionName,
    string City,
    string Zip,
    double Lat,
    double Lon,
    string Timezone,
    string Isp,
    string Org,
    string As,
    string Query
);
using System.Net;
using Microsoft.Extensions.Options;

namespace HalfLifeMetrics.Server.Options;

public sealed class RconOptionsConfiguration(IConfiguration configuration) : IConfigureOptions<RconOptions>, IValidateOptions<RconOptions>
{
    public void Configure(RconOptions options)
    {
        IConfigurationSection section = configuration.GetSection(nameof(RconOptions));
        
        options.ReceivePort = int.TryParse(section.GetValue<string>(nameof(RconOptions.ReceivePort)), out int port) ? port : 0;
        
        options.AllowedOrigins = section.GetSection(nameof(RconOptions.AllowedOrigins)).GetChildren().Select(x =>
        {
            if (x.GetValue<string>(nameof(AllowedOrigin.IpEndPoint)) is not { } ipEndPointString
                || !IPEndPoint.TryParse(ipEndPointString, out IPEndPoint? ipEndPoint)
                || x.GetValue<string>(nameof(AllowedOrigin.Password)) is not {} password
                || string.IsNullOrWhiteSpace(password))
            {
                return AllowedOrigin.Empty;
            }

            return new AllowedOrigin
            {
                Password = password,
                IpEndPoint = ipEndPoint
            };

        }).ToArray();
    }

    public ValidateOptionsResult Validate(string? name, RconOptions options)
    {
        List<string> errors = [];
        if (options.ReceivePort <= 0)
        {
            errors.Add($"{nameof(RconOptions.ReceivePort)} must be greater than 0");
        }

        if (options.AllowedOrigins.Length == 0)
        {
            errors.Add($"At least one {nameof(AllowedOrigin)} is required");
        }
        
        if (options.AllowedOrigins.Any(x => x == AllowedOrigin.Empty))
        {
            errors.Add($"One or more {nameof(AllowedOrigin)} is invalid");
        }

        if (options.AllowedOrigins.Any(x => string.IsNullOrWhiteSpace(x.Password)))
        {
            errors.Add($"One or more {nameof(AllowedOrigin)} passwords are empty");
        }
        
        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
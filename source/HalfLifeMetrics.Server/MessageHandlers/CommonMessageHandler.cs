using HalfLifeMetrics.Server.Worker;

namespace HalfLifeMetrics.Server.MessageHandlers;

public sealed class CommonMessageHandler(ILogger<RconListener> logger) : IRconMessageHandler
{
    public Task HandleMessage(string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Message: {Message}", message);
        return Task.CompletedTask;
    }
}
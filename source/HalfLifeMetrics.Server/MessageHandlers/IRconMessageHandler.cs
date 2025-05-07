namespace HalfLifeMetrics.Server.MessageHandlers;

public interface IRconMessageHandler
{
    Task HandleMessage(string message, CancellationToken cancellationToken);
}
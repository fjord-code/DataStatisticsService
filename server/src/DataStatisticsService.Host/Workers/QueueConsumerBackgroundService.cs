namespace DataStatisticsService.Host.Workers;

public sealed class QueueConsumerBackgroundService(ILogger<QueueConsumerBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queue consumer worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Placeholder loop for upcoming queue consumption implementation.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

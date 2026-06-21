using DataStatisticsService.Abstractions.Messaging;

namespace DataStatisticsService.Abstractions.Services;

public interface IStatisticsUpdatePublisher
{
    Task PublishAsync(StatisticsUpdatedMessage message, CancellationToken cancellationToken = default);
}

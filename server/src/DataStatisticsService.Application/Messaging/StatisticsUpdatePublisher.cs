using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Services;
using Wolverine;

namespace DataStatisticsService.Application.Messaging;

public sealed class StatisticsUpdatePublisher(IMessageBus messageBus) : IStatisticsUpdatePublisher
{
    public async Task PublishAsync(StatisticsUpdatedMessage message, CancellationToken cancellationToken = default)
    {
        await messageBus.PublishAsync(message);
    }
}

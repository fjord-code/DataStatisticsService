using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Application.Messaging;

public sealed class IngestedDataMessageHandler(ILogger<IngestedDataMessageHandler> logger)
{
    public async Task Handle(IngestedDataMessage message, IIngestedDataIngestionService ingestionService, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received ingested event {EventId} from RabbitMQ",
            message.EventId);

        await ingestionService.PersistAsync(message, cancellationToken);
    }
}

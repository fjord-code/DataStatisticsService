using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Service.Ingestion;

public sealed class IngestedDataIngestionService(
    IRawIngestedEventRepository repository,
    ILogger<IngestedDataIngestionService> logger) : IIngestedDataIngestionService
{
    public async Task<bool> PersistAsync(IngestedDataMessage message, CancellationToken cancellationToken = default)
    {
        var payloadJson = message.Payload.GetRawText();

        var inserted = await repository.AddAsync(
            message.EventId,
            message.Type,
            message.Name,
            payloadJson,
            DateTime.UtcNow,
            cancellationToken);

        if (inserted)
        {
            logger.LogInformation(
                "Persisted ingested event {EventId} (Type={Type}, Name={Name})",
                message.EventId,
                message.Type,
                message.Name);
        }

        return inserted;
    }
}

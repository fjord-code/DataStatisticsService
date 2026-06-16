using DataStatisticsService.Abstractions.Messaging;

namespace DataStatisticsService.Abstractions.Services;

public interface IIngestedDataIngestionService
{
    Task PersistAsync(IngestedDataMessage message, CancellationToken cancellationToken = default);
}

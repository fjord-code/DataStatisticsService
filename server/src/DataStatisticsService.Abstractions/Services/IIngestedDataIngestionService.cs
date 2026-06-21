using DataStatisticsService.Abstractions.Messaging;

namespace DataStatisticsService.Abstractions.Services;

public interface IIngestedDataIngestionService
{
    /// <returns><c>true</c> if the event was newly persisted; <c>false</c> if it was a duplicate.</returns>
    Task<bool> PersistAsync(IngestedDataMessage message, CancellationToken cancellationToken = default);
}

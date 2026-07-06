using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Queries;

namespace DataStatisticsService.Abstractions.Services;

public interface IStatisticsAggregationService
{
    Task<ReadingSnapshotDto> AggregateAsync(
        IngestedDataMessage message,
        CancellationToken cancellationToken = default);
}

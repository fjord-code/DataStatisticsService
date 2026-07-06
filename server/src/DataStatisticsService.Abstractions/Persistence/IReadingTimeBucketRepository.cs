using DataStatisticsService.Abstractions.Aggregation;

namespace DataStatisticsService.Abstractions.Persistence;

public interface IReadingTimeBucketRepository
{
    Task UpsertBucketAsync(
        string type,
        string name,
        DateTime bucketStartUtc,
        string granularity,
        ParsedReading parsed,
        CancellationToken cancellationToken = default);
}

using DataStatisticsService.Abstractions.Queries;

namespace DataStatisticsService.Abstractions.Persistence;

public interface IReadingQueryRepository
{
    Task<IReadOnlyList<ReadingSnapshotDto>> GetLatestSnapshotsAsync(
        ReadingQueryFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadingTimeBucketDto>> GetTimeBucketsAsync(
        string? type,
        string? name,
        DateTime fromUtc,
        DateTime toUtc,
        string granularity,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TypeAggregationDto>> GetAggregationsByTypeAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LocationAggregationDto>> GetAggregationsByLocationAsync(
        string? type = null,
        CancellationToken cancellationToken = default);
}

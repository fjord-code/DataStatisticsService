using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Abstractions.Persistence;

namespace DataStatisticsService.Service.Queries;

public sealed class ReadingQueryService(IReadingQueryRepository repository) : IReadingQueryService
{
    public Task<IReadOnlyList<ReadingSnapshotDto>> GetLatestSnapshotsAsync(
        ReadingQueryFilter filter,
        CancellationToken cancellationToken = default)
        => repository.GetLatestSnapshotsAsync(filter, cancellationToken);

    public Task<IReadOnlyList<ReadingTimeBucketDto>> GetTimeBucketsAsync(
        string? type,
        string? name,
        DateTime fromUtc,
        DateTime toUtc,
        string granularity,
        CancellationToken cancellationToken = default)
        => repository.GetTimeBucketsAsync(type, name, fromUtc, toUtc, granularity, cancellationToken);

    public Task<IReadOnlyList<TypeAggregationDto>> GetAggregationsByTypeAsync(
        CancellationToken cancellationToken = default)
        => repository.GetAggregationsByTypeAsync(cancellationToken);

    public Task<IReadOnlyList<LocationAggregationDto>> GetAggregationsByLocationAsync(
        string? type = null,
        CancellationToken cancellationToken = default)
        => repository.GetAggregationsByLocationAsync(type, cancellationToken);
}

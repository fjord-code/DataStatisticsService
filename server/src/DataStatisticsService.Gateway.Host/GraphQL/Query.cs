using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Gateway.Host.GraphQL;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;

namespace DataStatisticsService.Gateway.Host.GraphQL;

public sealed class Query
{
    [UseFiltering]
    [UseSorting]
    public async Task<IReadOnlyList<ReadingSnapshotDto>> GetLatestSnapshots(
        [Service] IReadingQueryService queryService,
        string? type,
        string? name,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
        => await queryService.GetLatestSnapshotsAsync(
            new ReadingQueryFilter(type, name, fromUtc, toUtc, skip, take),
            cancellationToken);

    public async Task<IReadOnlyList<ReadingTimeBucketDto>> GetReadingTimeBuckets(
        [Service] IReadingQueryService queryService,
        string? type,
        string? name,
        DateTime fromUtc,
        DateTime toUtc,
        string granularity = "hour",
        CancellationToken cancellationToken = default)
        => await queryService.GetTimeBucketsAsync(type, name, fromUtc, toUtc, granularity, cancellationToken);

    public Task<IReadOnlyList<TypeAggregationDto>> GetAggregationsByType(
        [Service] IReadingQueryService queryService,
        CancellationToken cancellationToken = default)
        => queryService.GetAggregationsByTypeAsync(cancellationToken);

    public Task<IReadOnlyList<LocationAggregationDto>> GetAggregationsByLocation(
        [Service] IReadingQueryService queryService,
        string? type = null,
        CancellationToken cancellationToken = default)
        => queryService.GetAggregationsByLocationAsync(type, cancellationToken);
}

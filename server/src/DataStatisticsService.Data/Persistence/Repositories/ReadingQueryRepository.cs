using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Data.Persistence;
using Microsoft.EntityFrameworkCore;

using DataStatisticsService.Abstractions.Persistence;

namespace DataStatisticsService.Data.Persistence.Repositories;

public sealed class ReadingQueryRepository(StatisticsDbContext dbContext) : IReadingQueryRepository
{
    public async Task<IReadOnlyList<ReadingSnapshotDto>> GetLatestSnapshotsAsync(
        ReadingQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReadingSnapshots.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            query = query.Where(x => x.Type == filter.Type);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            query = query.Where(x => x.Name == filter.Name);
        }

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(x => x.UpdatedAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(x => x.UpdatedAtUtc <= filter.ToUtc.Value);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .Select(x => new ReadingSnapshotDto(
                x.Type,
                x.Name,
                x.NumericValue,
                x.BoolValue,
                x.PayloadJson,
                x.LastEventId,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReadingTimeBucketDto>> GetTimeBucketsAsync(
        string? type,
        string? name,
        DateTime fromUtc,
        DateTime toUtc,
        string granularity,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReadingTimeBuckets.AsNoTracking()
            .Where(x => x.BucketStartUtc >= fromUtc && x.BucketStartUtc <= toUtc && x.Granularity == granularity);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name == name);
        }

        return await query
            .OrderBy(x => x.BucketStartUtc)
            .Select(x => new ReadingTimeBucketDto(
                x.Type,
                x.Name,
                x.BucketStartUtc,
                x.Granularity,
                x.SampleCount,
                x.SumNumeric,
                x.AvgNumeric,
                x.TrueCount,
                x.FalseCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TypeAggregationDto>> GetAggregationsByTypeAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ReadingSnapshots.AsNoTracking()
            .GroupBy(x => x.Type)
            .Select(g => new TypeAggregationDto(
                g.Key,
                g.Count(),
                g.Sum(x => x.NumericValue),
                g.Average(x => x.NumericValue)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LocationAggregationDto>> GetAggregationsByLocationAsync(
        string? type,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReadingSnapshots.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        return await query
            .GroupBy(x => x.Name)
            .Select(g => new LocationAggregationDto(
                g.Key,
                g.Count(),
                g.Sum(x => x.NumericValue),
                g.Average(x => x.NumericValue)))
            .ToListAsync(cancellationToken);
    }
}

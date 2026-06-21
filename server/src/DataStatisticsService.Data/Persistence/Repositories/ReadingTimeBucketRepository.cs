using DataStatisticsService.Data.Entities;
using DataStatisticsService.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataStatisticsService.Data.Persistence.Repositories;

public sealed class ReadingTimeBucketRepository(StatisticsDbContext dbContext) : Abstractions.Persistence.IReadingTimeBucketRepository
{
    public async Task UpsertBucketAsync(
        string type,
        string name,
        DateTime bucketStartUtc,
        string granularity,
        Abstractions.Aggregation.ParsedReading parsed,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ReadingTimeBuckets
            .FirstOrDefaultAsync(
                x => x.Type == type
                     && x.Name == name
                     && x.BucketStartUtc == bucketStartUtc
                     && x.Granularity == granularity,
                cancellationToken);

        if (existing is null)
        {
            dbContext.ReadingTimeBuckets.Add(CreateBucket(type, name, bucketStartUtc, granularity, parsed));
        }
        else
        {
            ApplySample(existing, parsed);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ReadingTimeBucket CreateBucket(
        string type,
        string name,
        DateTime bucketStartUtc,
        string granularity,
        Abstractions.Aggregation.ParsedReading parsed)
    {
        var bucket = new ReadingTimeBucket
        {
            Type = type,
            Name = name,
            BucketStartUtc = bucketStartUtc,
            Granularity = granularity,
            SampleCount = 0
        };

        ApplySample(bucket, parsed);
        return bucket;
    }

    private static void ApplySample(ReadingTimeBucket bucket, Abstractions.Aggregation.ParsedReading parsed)
    {
        bucket.SampleCount++;

        if (parsed.NumericValue.HasValue)
        {
            bucket.SumNumeric = (bucket.SumNumeric ?? 0) + parsed.NumericValue.Value;
            bucket.AvgNumeric = bucket.SumNumeric / bucket.SampleCount;
        }

        if (parsed.BoolValue.HasValue)
        {
            if (parsed.BoolValue.Value)
            {
                bucket.TrueCount = (bucket.TrueCount ?? 0) + 1;
            }
            else
            {
                bucket.FalseCount = (bucket.FalseCount ?? 0) + 1;
            }
        }
    }
}

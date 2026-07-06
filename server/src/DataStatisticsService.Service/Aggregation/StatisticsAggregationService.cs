using DataStatisticsService.Abstractions.Aggregation;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Service.Aggregation;

public sealed class StatisticsAggregationService(
    IReadingPayloadParser payloadParser,
    IReadingSnapshotRepository snapshotRepository,
    IReadingTimeBucketRepository timeBucketRepository,
    ILogger<StatisticsAggregationService> logger) : IStatisticsAggregationService
{
    private const string DefaultGranularity = "hour";

    public async Task<ReadingSnapshotDto> AggregateAsync(
        IngestedDataMessage message,
        CancellationToken cancellationToken = default)
    {
        var payloadJson = message.Payload.GetRawText();
        var parsed = payloadParser.Parse(message.Type, message.Payload);
        var updatedAtUtc = DateTime.UtcNow;
        var bucketStart = new DateTime(
            updatedAtUtc.Year,
            updatedAtUtc.Month,
            updatedAtUtc.Day,
            updatedAtUtc.Hour,
            0,
            0,
            DateTimeKind.Utc);

        await snapshotRepository.UpsertAsync(
            message.EventId,
            message.Type,
            message.Name,
            payloadJson,
            parsed,
            updatedAtUtc,
            cancellationToken);

        await timeBucketRepository.UpsertBucketAsync(
            message.Type,
            message.Name,
            bucketStart,
            DefaultGranularity,
            parsed,
            cancellationToken);

        logger.LogInformation(
            "Aggregated ingested event {EventId} (Type={Type}, Name={Name})",
            message.EventId,
            message.Type,
            message.Name);

        return new ReadingSnapshotDto(
            message.Type,
            message.Name,
            parsed.NumericValue,
            parsed.BoolValue,
            payloadJson,
            message.EventId,
            updatedAtUtc);
    }
}

using DataStatisticsService.Abstractions.Aggregation;

namespace DataStatisticsService.Abstractions.Persistence;

public interface IReadingSnapshotRepository
{
    Task UpsertAsync(
        Guid eventId,
        string type,
        string name,
        string payloadJson,
        ParsedReading parsed,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default);
}

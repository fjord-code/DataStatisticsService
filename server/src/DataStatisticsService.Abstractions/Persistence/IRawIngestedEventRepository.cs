namespace DataStatisticsService.Abstractions.Persistence;

public interface IRawIngestedEventRepository
{
    Task AddAsync(
        Guid eventId,
        string type,
        string name,
        string payloadJson,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default);
}

namespace DataStatisticsService.Abstractions.Persistence;

public interface IRawIngestedEventRepository
{
    /// <returns><c>true</c> if a new row was inserted; <c>false</c> if the event id already existed.</returns>
    Task<bool> AddAsync(
        Guid eventId,
        string type,
        string name,
        string payloadJson,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default);
}

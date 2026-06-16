using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Data.Entities;

namespace DataStatisticsService.Data.Persistence.Repositories;

public sealed class RawIngestedEventRepository(StatisticsDbContext dbContext) : IRawIngestedEventRepository
{
    public async Task AddAsync(
        Guid eventId,
        string type,
        string name,
        string payloadJson,
        DateTime receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var entity = new RawIngestedEvent
        {
            EventId = eventId,
            Type = type,
            Name = name,
            PayloadJson = payloadJson,
            ReceivedAtUtc = receivedAtUtc
        };

        dbContext.RawIngestedEvents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

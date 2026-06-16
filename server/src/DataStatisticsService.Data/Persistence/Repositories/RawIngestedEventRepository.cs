using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DataStatisticsService.Data.Persistence.Repositories;

public sealed class RawIngestedEventRepository(
    StatisticsDbContext dbContext,
    ILogger<RawIngestedEventRepository> logger) : IRawIngestedEventRepository
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

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            dbContext.Entry(entity).State = EntityState.Detached;

            logger.LogInformation(
                "Duplicate ingested event {EventId} ignored (already persisted)",
                eventId);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return true;
            }
        }

        return false;
    }
}

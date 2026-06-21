using DataStatisticsService.Data.Entities;
using DataStatisticsService.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataStatisticsService.Data.Persistence.Repositories;

public sealed class ReadingSnapshotRepository(StatisticsDbContext dbContext) : Abstractions.Persistence.IReadingSnapshotRepository
{
    public async Task UpsertAsync(
        Guid eventId,
        string type,
        string name,
        string payloadJson,
        Abstractions.Aggregation.ParsedReading parsed,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.ReadingSnapshots
            .FirstOrDefaultAsync(x => x.Type == type && x.Name == name, cancellationToken);

        if (existing is null)
        {
            dbContext.ReadingSnapshots.Add(new ReadingSnapshot
            {
                Type = type,
                Name = name,
                NumericValue = parsed.NumericValue,
                BoolValue = parsed.BoolValue,
                PayloadJson = payloadJson,
                LastEventId = eventId,
                UpdatedAtUtc = updatedAtUtc
            });
        }
        else
        {
            existing.NumericValue = parsed.NumericValue;
            existing.BoolValue = parsed.BoolValue;
            existing.PayloadJson = payloadJson;
            existing.LastEventId = eventId;
            existing.UpdatedAtUtc = updatedAtUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

namespace DataStatisticsService.Data.Entities;

public sealed class RawIngestedEvent
{
    public long Id { get; set; }

    public Guid EventId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; }
}

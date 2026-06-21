namespace DataStatisticsService.Data.Entities;

public sealed class ReadingSnapshot
{
    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double? NumericValue { get; set; }

    public bool? BoolValue { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public Guid LastEventId { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

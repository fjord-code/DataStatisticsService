namespace DataStatisticsService.Abstractions.Messaging;

public sealed record StatisticsUpdatedMessage
{
    public Guid EventId { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public double? NumericValue { get; init; }

    public bool? BoolValue { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}

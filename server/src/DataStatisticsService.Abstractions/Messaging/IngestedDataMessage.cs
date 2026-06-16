using System.Text.Json;

namespace DataStatisticsService.Abstractions.Messaging;

public sealed record IngestedDataMessage
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public string Type { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public JsonElement Payload { get; init; } = default;
}

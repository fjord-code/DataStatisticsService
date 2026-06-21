using System.Text.Json;
using DataStatisticsService.Abstractions.Aggregation;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Service.Aggregation;

public sealed class ReadingPayloadParser(ILogger<ReadingPayloadParser> logger) : IReadingPayloadParser
{
    public ParsedReading Parse(string type, JsonElement payload)
    {
        return type.ToLowerInvariant() switch
        {
            "energy" when payload.TryGetProperty("energy", out var energy) && energy.TryGetDouble(out var value)
                => new ParsedReading(value, null),
            "motion" when payload.TryGetProperty("motionDetected", out var motion) && motion.ValueKind is JsonValueKind.True or JsonValueKind.False
                => new ParsedReading(null, motion.GetBoolean()),
            _ => ParseGeneric(payload, type)
        };
    }

    private ParsedReading ParseGeneric(JsonElement payload, string type)
    {
        if (payload.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in payload.EnumerateObject())
            {
                if (property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    return new ParsedReading(null, property.Value.GetBoolean());
                }

                if (property.Value.TryGetDouble(out var numeric))
                {
                    return new ParsedReading(numeric, null);
                }
            }
        }

        logger.LogDebug("No known payload shape for type {Type}", type);
        return new ParsedReading(null, null);
    }
}

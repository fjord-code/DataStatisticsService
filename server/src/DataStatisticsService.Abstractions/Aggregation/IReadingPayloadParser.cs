using System.Text.Json;

namespace DataStatisticsService.Abstractions.Aggregation;

public interface IReadingPayloadParser
{
    ParsedReading Parse(string type, JsonElement payload);
}

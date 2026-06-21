using System.Text.Json;
using DataStatisticsService.Abstractions.Aggregation;
using DataStatisticsService.Service.Aggregation;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataStatisticsService.Service.Tests.Aggregation;

public sealed class ReadingPayloadParserTests
{
    private readonly ReadingPayloadParser _parser = new(NullLogger<ReadingPayloadParser>.Instance);

    [Fact]
    public void Parse_EnergyPayload_ReturnsNumericValue()
    {
        var payload = JsonDocument.Parse("""{"energy":795.61}""").RootElement;

        var result = _parser.Parse("energy", payload);

        Assert.Equal(795.61, result.NumericValue);
        Assert.Null(result.BoolValue);
    }

    [Fact]
    public void Parse_MotionPayload_ReturnsBoolValue()
    {
        var payload = JsonDocument.Parse("""{"motionDetected":false}""").RootElement;

        var result = _parser.Parse("motion", payload);

        Assert.Null(result.NumericValue);
        Assert.False(result.BoolValue);
    }

    [Fact]
    public void Parse_GenericNumericPayload_ReturnsFirstNumericProperty()
    {
        var payload = JsonDocument.Parse("""{"value":42}""").RootElement;

        var result = _parser.Parse("temperature", payload);

        Assert.Equal(42, result.NumericValue);
    }
}

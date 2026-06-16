using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Services;
using DataStatisticsService.Application.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataStatisticsService.Application.Tests.Messaging;

public sealed class IngestedDataMessageHandlerTests
{
    private readonly IngestedDataMessageHandler _handler = new(NullLogger<IngestedDataMessageHandler>.Instance);

    [Fact]
    public async Task Handle_ValidMessage_CallsPersistAsyncOnce()
    {
        var ingestionService = new FakeIngestionService();
        var message = CreateValidMessage();

        await _handler.Handle(message, ingestionService, CancellationToken.None);

        Assert.Equal(1, ingestionService.PersistCallCount);
        Assert.Same(message, ingestionService.LastMessage);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "type", "name", "{}", "non-empty EventId")]
    [InlineData("11111111-1111-1111-1111-111111111111", "", "name", "{}", "non-empty Type")]
    [InlineData("11111111-1111-1111-1111-111111111111", "   ", "name", "{}", "non-empty Type")]
    [InlineData("11111111-1111-1111-1111-111111111111", "type", "", "{}", "non-empty Name")]
    [InlineData("11111111-1111-1111-1111-111111111111", "type", "   ", "{}", "non-empty Name")]
    public async Task Handle_InvalidMessage_ThrowsAndDoesNotPersist(
        string eventId,
        string type,
        string name,
        string payloadJson,
        string _)
    {
        var ingestionService = new FakeIngestionService();
        var message = new IngestedDataMessage
        {
            EventId = Guid.Parse(eventId),
            Type = type,
            Name = name,
            Payload = JsonDocument.Parse(payloadJson).RootElement
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, CancellationToken.None));

        Assert.NotNull(exception.Message);
        Assert.Equal(0, ingestionService.PersistCallCount);
    }

    [Fact]
    public async Task Handle_UndefinedPayload_ThrowsAndDoesNotPersist()
    {
        var ingestionService = new FakeIngestionService();
        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "type",
            Name = "name"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, CancellationToken.None));

        Assert.Equal(0, ingestionService.PersistCallCount);
    }

    [Fact]
    public async Task Handle_NullPayload_ThrowsAndDoesNotPersist()
    {
        var ingestionService = new FakeIngestionService();
        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "type",
            Name = "name",
            Payload = JsonDocument.Parse("null").RootElement
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, CancellationToken.None));

        Assert.Equal(0, ingestionService.PersistCallCount);
    }

    private static IngestedDataMessage CreateValidMessage() =>
        new()
        {
            EventId = Guid.NewGuid(),
            Type = "temperature",
            Name = "sensor-1",
            Payload = JsonDocument.Parse("""{"value":42}""").RootElement
        };

    private sealed class FakeIngestionService : IIngestedDataIngestionService
    {
        public int PersistCallCount { get; private set; }

        public IngestedDataMessage? LastMessage { get; private set; }

        public Task PersistAsync(IngestedDataMessage message, CancellationToken cancellationToken = default)
        {
            PersistCallCount++;
            LastMessage = message;
            return Task.CompletedTask;
        }
    }
}

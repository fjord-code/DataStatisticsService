using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Abstractions.Services;
using DataStatisticsService.Application.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataStatisticsService.Application.Tests.Messaging;

public sealed class IngestedDataMessageHandlerTests
{
    private readonly IngestedDataMessageHandler _handler = new(NullLogger<IngestedDataMessageHandler>.Instance);

    [Fact]
    public async Task Handle_ValidMessage_CallsPersistAggregateAndPublish()
    {
        var ingestionService = new FakeIngestionService();
        var aggregationService = new FakeAggregationService();
        var updatePublisher = new FakeUpdatePublisher();
        var message = CreateValidMessage();

        await _handler.Handle(message, ingestionService, aggregationService, updatePublisher, CancellationToken.None);

        Assert.Equal(1, ingestionService.PersistCallCount);
        Assert.Same(message, ingestionService.LastMessage);
        Assert.Equal(1, aggregationService.AggregateCallCount);
        Assert.Equal(1, updatePublisher.PublishCallCount);
    }

    [Fact]
    public async Task Handle_DuplicatePersist_SkipsAggregateAndPublish()
    {
        var ingestionService = new FakeIngestionService(inserted: false);
        var aggregationService = new FakeAggregationService();
        var updatePublisher = new FakeUpdatePublisher();
        var message = CreateValidMessage();

        await _handler.Handle(message, ingestionService, aggregationService, updatePublisher, CancellationToken.None);

        Assert.Equal(1, ingestionService.PersistCallCount);
        Assert.Equal(0, aggregationService.AggregateCallCount);
        Assert.Equal(0, updatePublisher.PublishCallCount);
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
        var aggregationService = new FakeAggregationService();
        var updatePublisher = new FakeUpdatePublisher();
        var message = new IngestedDataMessage
        {
            EventId = Guid.Parse(eventId),
            Type = type,
            Name = name,
            Payload = JsonDocument.Parse(payloadJson).RootElement
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, aggregationService, updatePublisher, CancellationToken.None));

        Assert.NotNull(exception.Message);
        Assert.Equal(0, ingestionService.PersistCallCount);
        Assert.Equal(0, aggregationService.AggregateCallCount);
        Assert.Equal(0, updatePublisher.PublishCallCount);
    }

    [Fact]
    public async Task Handle_UndefinedPayload_ThrowsAndDoesNotPersist()
    {
        var ingestionService = new FakeIngestionService();
        var aggregationService = new FakeAggregationService();
        var updatePublisher = new FakeUpdatePublisher();
        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "type",
            Name = "name"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, aggregationService, updatePublisher, CancellationToken.None));

        Assert.Equal(0, ingestionService.PersistCallCount);
    }

    [Fact]
    public async Task Handle_NullPayload_ThrowsAndDoesNotPersist()
    {
        var ingestionService = new FakeIngestionService();
        var aggregationService = new FakeAggregationService();
        var updatePublisher = new FakeUpdatePublisher();
        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "type",
            Name = "name",
            Payload = JsonDocument.Parse("null").RootElement
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(message, ingestionService, aggregationService, updatePublisher, CancellationToken.None));

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

    private sealed class FakeIngestionService(bool inserted = true) : IIngestedDataIngestionService
    {
        public int PersistCallCount { get; private set; }

        public IngestedDataMessage? LastMessage { get; private set; }

        public Task<bool> PersistAsync(IngestedDataMessage message, CancellationToken cancellationToken = default)
        {
            PersistCallCount++;
            LastMessage = message;
            return Task.FromResult(inserted);
        }
    }

    private sealed class FakeAggregationService : IStatisticsAggregationService
    {
        public int AggregateCallCount { get; private set; }

        public Task<ReadingSnapshotDto> AggregateAsync(
            IngestedDataMessage message,
            CancellationToken cancellationToken = default)
        {
            AggregateCallCount++;
            return Task.FromResult(new ReadingSnapshotDto(
                message.Type,
                message.Name,
                42,
                null,
                message.Payload.GetRawText(),
                message.EventId,
                DateTime.UtcNow));
        }
    }

    private sealed class FakeUpdatePublisher : IStatisticsUpdatePublisher
    {
        public int PublishCallCount { get; private set; }

        public Task PublishAsync(StatisticsUpdatedMessage message, CancellationToken cancellationToken = default)
        {
            PublishCallCount++;
            return Task.CompletedTask;
        }
    }
}

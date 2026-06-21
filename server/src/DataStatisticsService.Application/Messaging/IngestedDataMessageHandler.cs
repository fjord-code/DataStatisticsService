using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wolverine.ErrorHandling;
using Wolverine.Runtime.Handlers;

namespace DataStatisticsService.Application.Messaging;

public sealed class IngestedDataMessageHandler(ILogger<IngestedDataMessageHandler> logger)
{
    public static void Configure(HandlerChain chain)
    {
        chain.OnException<InvalidOperationException>()
            .MoveToErrorQueue();

        chain.OnException<NpgsqlException>()
            .Requeue(3)
            .Then.MoveToErrorQueue();

        chain.OnException<TimeoutException>()
            .Requeue(3)
            .Then.MoveToErrorQueue();

        chain.OnException<Exception>()
            .Requeue(3)
            .Then.MoveToErrorQueue();
    }

    public async Task Handle(
        IngestedDataMessage message,
        IIngestedDataIngestionService ingestionService,
        IStatisticsAggregationService aggregationService,
        IStatisticsUpdatePublisher updatePublisher,
        CancellationToken cancellationToken)
    {
        ValidateMessage(message);

        logger.LogInformation(
            "Received ingested event {EventId} from RabbitMQ",
            message.EventId);

        await ingestionService.PersistAsync(message, cancellationToken);

        var snapshot = await aggregationService.AggregateAsync(message, cancellationToken);

        await updatePublisher.PublishAsync(
            new StatisticsUpdatedMessage
            {
                EventId = snapshot.LastEventId,
                Type = snapshot.Type,
                Name = snapshot.Name,
                NumericValue = snapshot.NumericValue,
                BoolValue = snapshot.BoolValue,
                UpdatedAtUtc = snapshot.UpdatedAtUtc
            },
            cancellationToken);

        logger.LogInformation(
            "Published statistics update for event {EventId}",
            message.EventId);
    }

    private static void ValidateMessage(IngestedDataMessage message)
    {
        if (message.EventId == Guid.Empty)
        {
            throw new InvalidOperationException("Ingested event must have a non-empty EventId.");
        }

        if (string.IsNullOrWhiteSpace(message.Type))
        {
            throw new InvalidOperationException("Ingested event must have a non-empty Type.");
        }

        if (string.IsNullOrWhiteSpace(message.Name))
        {
            throw new InvalidOperationException("Ingested event must have a non-empty Name.");
        }

        if (message.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new InvalidOperationException("Ingested event must have a defined Payload.");
        }

        try
        {
            _ = message.Payload.GetRawText();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ingested event Payload is not valid JSON.", ex);
        }
    }
}

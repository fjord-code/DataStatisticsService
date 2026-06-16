using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Application.Messaging;

public sealed class IngestedDataMessageHandler(ILogger<IngestedDataMessageHandler> logger)
{
    public async Task Handle(IngestedDataMessage message, IIngestedDataIngestionService ingestionService, CancellationToken cancellationToken)
    {
        ValidateMessage(message);

        logger.LogInformation(
            "Received ingested event {EventId} from RabbitMQ",
            message.EventId);

        await ingestionService.PersistAsync(message, cancellationToken);
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

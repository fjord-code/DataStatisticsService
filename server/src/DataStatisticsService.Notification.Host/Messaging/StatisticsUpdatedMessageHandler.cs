using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Notification.Host.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Notification.Host.Messaging;

public sealed class StatisticsUpdatedMessageHandler(
    IHubContext<StatisticsHub> hubContext,
    ILogger<StatisticsUpdatedMessageHandler> logger)
{
    public async Task Handle(StatisticsUpdatedMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Broadcasting statistics update for {Type}/{Name}",
            message.Type,
            message.Name);

        await hubContext.Clients.All.SendAsync(
            "ReadingUpdated",
            new
            {
                message.EventId,
                message.Type,
                message.Name,
                message.NumericValue,
                message.BoolValue,
                message.UpdatedAtUtc
            },
            cancellationToken);
    }
}

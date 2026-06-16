using DataStatisticsService.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace DataStatisticsService.Service.Aggregation;

public sealed class StatisticsAggregationService(ILogger<StatisticsAggregationService> logger) : IStatisticsAggregationService
{
    public Task AggregateAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Aggregation placeholder executed.");
        return Task.CompletedTask;
    }
}

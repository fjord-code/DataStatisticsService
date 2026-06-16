namespace DataStatisticsService.Abstractions.Services;

public interface IStatisticsAggregationService
{
    Task AggregateAsync(CancellationToken cancellationToken = default);
}

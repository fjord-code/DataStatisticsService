using DataStatisticsService.Abstractions.Aggregation;
using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Abstractions.Services;
using DataStatisticsService.Service.Aggregation;
using DataStatisticsService.Service.Ingestion;
using DataStatisticsService.Service.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace DataStatisticsService.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IStatisticsAggregationService, StatisticsAggregationService>();
        services.AddScoped<IIngestedDataIngestionService, IngestedDataIngestionService>();
        services.AddScoped<IReadingPayloadParser, ReadingPayloadParser>();
        services.AddScoped<IReadingQueryService, ReadingQueryService>();
        return services;
    }
}

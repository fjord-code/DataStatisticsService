using Microsoft.Extensions.DependencyInjection;
using DataStatisticsService.Abstractions.Services;
using DataStatisticsService.Application.Messaging;

namespace DataStatisticsService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IStatisticsUpdatePublisher, StatisticsUpdatePublisher>();
        return services;
    }
}

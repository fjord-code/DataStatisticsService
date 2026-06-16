using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Data.Configuration;
using DataStatisticsService.Data.Persistence;
using DataStatisticsService.Data.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataStatisticsService.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddDbContext<StatisticsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("StatisticsDb"));
        });
        services.AddScoped<IRawIngestedEventRepository, RawIngestedEventRepository>();

        return services;
    }
}

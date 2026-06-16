using DataStatisticsService.Application;
using DataStatisticsService.Data;
using DataStatisticsService.Host.HealthChecks;
using DataStatisticsService.Host.Workers;
using DataStatisticsService.Service;

namespace DataStatisticsService.Host.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services
            .AddApplicationLayer()
            .AddServiceLayer()
            .AddDataLayer(builder.Configuration);

        builder.Services.AddHealthChecks().AddCheck<ServiceReadinessHealthCheck>("service_readiness");
        builder.Services.AddHostedService<QueueConsumerBackgroundService>();

        return builder;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}

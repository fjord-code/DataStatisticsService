using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Application;
using DataStatisticsService.Application.Messaging;
using DataStatisticsService.Data.Configuration;
using DataStatisticsService.Data;
using DataStatisticsService.Host.HealthChecks;
using DataStatisticsService.Data.Persistence;
using DataStatisticsService.Service;
using JasperFx.CodeGeneration.Model;
using Wolverine;
using Wolverine.RabbitMQ;

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

        var rabbitMqOptions = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        builder.Host.UseWolverine(options =>
        {
            options.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;
            options.Discovery.IncludeAssembly(typeof(IngestedDataMessageHandler).Assembly);

            options
                .UseRabbitMq(c =>
                {
                    c.HostName = rabbitMqOptions.HostName;
                    c.Port = rabbitMqOptions.Port;
                    c.UserName = rabbitMqOptions.UserName;
                    c.Password = rabbitMqOptions.Password;
                })
                .AutoProvision();

            options.ListenToRabbitQueue(rabbitMqOptions.QueueName)
                .DefaultIncomingMessage<IngestedDataMessage>();

            options.PublishMessage<StatisticsUpdatedMessage>()
                .ToRabbitExchange(rabbitMqOptions.StatisticsUpdatesExchange);
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck<InfrastructureReadinessHealthCheck>("infrastructure_readiness", tags: ["ready"]);

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

        app.MapGet("/health/live", () => Results.Ok());
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        app.MapControllers();

        return app;
    }
}

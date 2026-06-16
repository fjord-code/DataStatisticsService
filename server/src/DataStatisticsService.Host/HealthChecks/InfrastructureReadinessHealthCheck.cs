using DataStatisticsService.Data.Configuration;
using DataStatisticsService.Data.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DataStatisticsService.Host.HealthChecks;

public sealed class InfrastructureReadinessHealthCheck(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> rabbitMqOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StatisticsDbContext>();
        var dbConnected = await dbContext.Database.CanConnectAsync(cancellationToken);

        if (!dbConnected)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.");
        }

        var options = rabbitMqOptions.Value;
        var connectionFactory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        if (!connection.IsOpen)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection failed.");
        }

        return HealthCheckResult.Healthy("Database and RabbitMQ are reachable.");
    }
}

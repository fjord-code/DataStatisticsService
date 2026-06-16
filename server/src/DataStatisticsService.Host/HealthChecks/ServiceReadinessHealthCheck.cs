using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataStatisticsService.Host.HealthChecks;

public sealed class ServiceReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("DataStatisticsService host is ready."));
    }
}

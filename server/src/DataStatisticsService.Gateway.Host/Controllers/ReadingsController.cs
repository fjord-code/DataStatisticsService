using DataStatisticsService.Abstractions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace DataStatisticsService.Gateway.Host.Controllers;

[ApiController]
[Route("api/readings")]
public sealed class ReadingsController(IReadingQueryService queryService) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<IReadOnlyList<ReadingSnapshotDto>> GetLatest(
        [FromQuery] string? type,
        [FromQuery] string? name,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => await queryService.GetLatestSnapshotsAsync(
            new ReadingQueryFilter(type, name, Skip: skip, Take: take),
            cancellationToken);

    [HttpGet("aggregations/by-type")]
    public Task<IReadOnlyList<TypeAggregationDto>> GetByType(CancellationToken cancellationToken = default)
        => queryService.GetAggregationsByTypeAsync(cancellationToken);

    [HttpGet("aggregations/by-location")]
    public Task<IReadOnlyList<LocationAggregationDto>> GetByLocation(
        [FromQuery] string? type,
        CancellationToken cancellationToken = default)
        => queryService.GetAggregationsByLocationAsync(type, cancellationToken);

    [HttpGet("timeseries")]
    public Task<IReadOnlyList<ReadingTimeBucketDto>> GetTimeSeries(
        [FromQuery] string? type,
        [FromQuery] string? name,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] string granularity = "hour",
        CancellationToken cancellationToken = default)
        => queryService.GetTimeBucketsAsync(type, name, fromUtc, toUtc, granularity, cancellationToken);
}

using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Data.Persistence;
using DataStatisticsService.Data.Persistence.Repositories;
using DataStatisticsService.Service.Aggregation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace DataStatisticsService.Data.Tests.Persistence;

public sealed class AggregationIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("datastatistics")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [Fact]
    public async Task AggregateAsync_creates_snapshot_and_time_bucket()
    {
        await using var context = CreateContext();
        var snapshotRepository = new ReadingSnapshotRepository(context);
        var bucketRepository = new ReadingTimeBucketRepository(context);
        var parser = new ReadingPayloadParser(NullLogger<ReadingPayloadParser>.Instance);
        var aggregationService = new StatisticsAggregationService(
            parser,
            snapshotRepository,
            bucketRepository,
            NullLogger<StatisticsAggregationService>.Instance);

        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "energy",
            Name = "Garage",
            Payload = JsonDocument.Parse("""{"energy":100.5}""").RootElement
        };

        await aggregationService.AggregateAsync(message);

        var snapshot = await context.ReadingSnapshots.SingleAsync(x => x.Type == "energy" && x.Name == "Garage");
        Assert.Equal(100.5, snapshot.NumericValue);
        Assert.Equal(1, await context.ReadingTimeBuckets.CountAsync(x => x.Type == "energy" && x.Name == "Garage"));
    }

    private StatisticsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<StatisticsDbContext>()
            .UseNpgsql(
                _postgres.GetConnectionString(),
                npgsql => npgsql.MigrationsAssembly(typeof(StatisticsDbContext).Assembly.GetName().Name))
            .Options;

        return new StatisticsDbContext(options);
    }
}

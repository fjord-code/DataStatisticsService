using DataStatisticsService.Data.Persistence;
using DataStatisticsService.Data.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace DataStatisticsService.Data.Tests.Persistence;

public sealed class RawIngestedEventRepositoryTests : IAsyncLifetime
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
    public async Task AddAsync_persists_new_event()
    {
        await using var context = CreateContext();
        var repository = new RawIngestedEventRepository(context, NullLogger<RawIngestedEventRepository>.Instance);
        var eventId = Guid.NewGuid();

        await repository.AddAsync(
            eventId,
            "temperature",
            "sensor-1",
            """{"value":42}""",
            DateTime.UtcNow);

        var persisted = await context.RawIngestedEvents.SingleOrDefaultAsync(e => e.EventId == eventId);

        Assert.NotNull(persisted);
        Assert.Equal("temperature", persisted.Type);
        Assert.Equal("sensor-1", persisted.Name);
        Assert.Equal("""{"value":42}""", persisted.PayloadJson);
    }

    [Fact]
    public async Task AddAsync_duplicate_event_id_is_ignored()
    {
        await using var context = CreateContext();
        var repository = new RawIngestedEventRepository(context, NullLogger<RawIngestedEventRepository>.Instance);
        var eventId = Guid.NewGuid();

        await repository.AddAsync(
            eventId,
            "temperature",
            "sensor-1",
            """{"value":1}""",
            DateTime.UtcNow);

        await repository.AddAsync(
            eventId,
            "temperature",
            "sensor-1",
            """{"value":2}""",
            DateTime.UtcNow);

        Assert.Equal(1, await context.RawIngestedEvents.CountAsync(e => e.EventId == eventId));

        var persisted = await context.RawIngestedEvents.SingleAsync(e => e.EventId == eventId);
        Assert.Equal("""{"value":1}""", persisted.PayloadJson);
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

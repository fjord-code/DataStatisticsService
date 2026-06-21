using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataStatisticsService.Abstractions.Messaging;
using DataStatisticsService.Abstractions.Persistence;
using DataStatisticsService.Abstractions.Queries;
using DataStatisticsService.Abstractions.Services;
using DataStatisticsService.Application.Messaging;
using DataStatisticsService.Data.Persistence;
using DataStatisticsService.Data.Persistence.Repositories;
using DataStatisticsService.Service;
using DataStatisticsService.Service.Aggregation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace DataStatisticsService.Host.Tests.Integration;

public sealed class IngestedDataPipelineIntegrationTests : IAsyncLifetime
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
    public async Task Handle_persists_raw_event_and_aggregates()
    {
        await using var context = CreateContext();
        var services = BuildServices(context);
        var handler = new IngestedDataMessageHandler(NullLogger<IngestedDataMessageHandler>.Instance);
        var message = new IngestedDataMessage
        {
            EventId = Guid.NewGuid(),
            Type = "energy",
            Name = "Garage",
            Payload = JsonDocument.Parse("""{"energy":42.5}""").RootElement
        };

        await handler.Handle(
            message,
            services.GetRequiredService<IIngestedDataIngestionService>(),
            services.GetRequiredService<IStatisticsAggregationService>(),
            services.GetRequiredService<IStatisticsUpdatePublisher>(),
            CancellationToken.None);

        Assert.Equal(1, await context.RawIngestedEvents.CountAsync(x => x.EventId == message.EventId));
        var snapshot = await context.ReadingSnapshots.SingleAsync(x => x.Type == "energy" && x.Name == "Garage");
        Assert.Equal(42.5, snapshot.NumericValue);
    }

    private static ServiceProvider BuildServices(StatisticsDbContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(context);
        services.AddScoped<IRawIngestedEventRepository, RawIngestedEventRepository>();
        services.AddScoped<IReadingSnapshotRepository, ReadingSnapshotRepository>();
        services.AddScoped<IReadingTimeBucketRepository, ReadingTimeBucketRepository>();
        services.AddServiceLayer();
        services.AddScoped<IStatisticsUpdatePublisher, FakeUpdatePublisher>();
        return services.BuildServiceProvider();
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

    private sealed class FakeUpdatePublisher : Abstractions.Services.IStatisticsUpdatePublisher
    {
        public Task PublishAsync(StatisticsUpdatedMessage message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

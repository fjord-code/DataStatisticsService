using System.Net;
using System.Text.Json;
using DataStatisticsService.Data.Entities;
using DataStatisticsService.Data.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace DataStatisticsService.Gateway.Tests;

public sealed class ReadingsApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("datastatistics")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StatisticsDbContext>));
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<StatisticsDbContext>(options =>
                        options.UseNpgsql(
                            _postgres.GetConnectionString(),
                            npgsql => npgsql.MigrationsAssembly(typeof(StatisticsDbContext).Assembly.GetName().Name)));
                });
            });

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StatisticsDbContext>();
        await db.Database.MigrateAsync();
        db.ReadingSnapshots.Add(new ReadingSnapshot
        {
            Type = "energy",
            Name = "Garage",
            NumericValue = 100,
            PayloadJson = """{"energy":100}""",
            LastEventId = Guid.NewGuid(),
            UpdatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task GetLatest_returns_seeded_snapshot()
    {
        var client = _factory!.CreateClient();
        var response = await client.GetAsync("/api/readings/latest");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(1, document.RootElement.GetArrayLength());
        Assert.Equal("Garage", document.RootElement[0].GetProperty("name").GetString());
    }
}

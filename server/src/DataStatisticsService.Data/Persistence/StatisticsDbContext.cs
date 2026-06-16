using DataStatisticsService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStatisticsService.Data.Persistence;

public sealed class StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : DbContext(options)
{
    public DbSet<RawIngestedEvent> RawIngestedEvents => Set<RawIngestedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RawIngestedEvent>();
        entity.ToTable("raw_ingested_events");

        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.EventId).IsUnique();
        entity.Property(x => x.Type).HasMaxLength(128).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        entity.Property(x => x.ReceivedAtUtc).IsRequired();
    }
}

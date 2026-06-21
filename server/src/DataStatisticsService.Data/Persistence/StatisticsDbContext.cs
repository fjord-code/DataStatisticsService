using DataStatisticsService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataStatisticsService.Data.Persistence;

public sealed class StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : DbContext(options)
{
    public DbSet<RawIngestedEvent> RawIngestedEvents => Set<RawIngestedEvent>();

    public DbSet<ReadingSnapshot> ReadingSnapshots => Set<ReadingSnapshot>();

    public DbSet<ReadingTimeBucket> ReadingTimeBuckets => Set<ReadingTimeBucket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var raw = modelBuilder.Entity<RawIngestedEvent>();
        raw.ToTable("raw_ingested_events");
        raw.HasKey(x => x.Id);
        raw.HasIndex(x => x.EventId).IsUnique();
        raw.Property(x => x.Type).HasMaxLength(128).IsRequired();
        raw.Property(x => x.Name).HasMaxLength(256).IsRequired();
        raw.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        raw.Property(x => x.ReceivedAtUtc).IsRequired();

        var snapshot = modelBuilder.Entity<ReadingSnapshot>();
        snapshot.ToTable("reading_snapshots");
        snapshot.HasKey(x => x.Id);
        snapshot.HasIndex(x => new { x.Type, x.Name }).IsUnique();
        snapshot.Property(x => x.Type).HasMaxLength(128).IsRequired();
        snapshot.Property(x => x.Name).HasMaxLength(256).IsRequired();
        snapshot.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        snapshot.Property(x => x.LastEventId).IsRequired();
        snapshot.Property(x => x.UpdatedAtUtc).IsRequired();

        var bucket = modelBuilder.Entity<ReadingTimeBucket>();
        bucket.ToTable("reading_time_buckets");
        bucket.HasKey(x => x.Id);
        bucket.HasIndex(x => new { x.Type, x.Name, x.BucketStartUtc, x.Granularity }).IsUnique();
        bucket.Property(x => x.Type).HasMaxLength(128).IsRequired();
        bucket.Property(x => x.Name).HasMaxLength(256).IsRequired();
        bucket.Property(x => x.Granularity).HasMaxLength(32).IsRequired();
        bucket.Property(x => x.BucketStartUtc).IsRequired();
    }
}

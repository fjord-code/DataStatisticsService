namespace DataStatisticsService.Data.Entities;

public sealed class ReadingTimeBucket
{
    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime BucketStartUtc { get; set; }

    public string Granularity { get; set; } = "hour";

    public int SampleCount { get; set; }

    public double? SumNumeric { get; set; }

    public double? AvgNumeric { get; set; }

    public int? TrueCount { get; set; }

    public int? FalseCount { get; set; }
}

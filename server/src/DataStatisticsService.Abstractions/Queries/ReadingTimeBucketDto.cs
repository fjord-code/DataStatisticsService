namespace DataStatisticsService.Abstractions.Queries;

public sealed record ReadingTimeBucketDto(
    string Type,
    string Name,
    DateTime BucketStartUtc,
    string Granularity,
    int SampleCount,
    double? SumNumeric,
    double? AvgNumeric,
    int? TrueCount,
    int? FalseCount);

namespace DataStatisticsService.Abstractions.Queries;

public sealed record ReadingSnapshotDto(
    string Type,
    string Name,
    double? NumericValue,
    bool? BoolValue,
    string PayloadJson,
    Guid LastEventId,
    DateTime UpdatedAtUtc);

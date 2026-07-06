namespace DataStatisticsService.Abstractions.Queries;

public sealed record ReadingQueryFilter(
    string? Type = null,
    string? Name = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Skip = 0,
    int Take = 50);

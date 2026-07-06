namespace DataStatisticsService.Abstractions.Queries;

public sealed record TypeAggregationDto(
    string Type,
    int Count,
    double? SumNumeric,
    double? AvgNumeric);

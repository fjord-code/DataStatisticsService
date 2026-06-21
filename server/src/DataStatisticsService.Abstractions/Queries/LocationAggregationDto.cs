namespace DataStatisticsService.Abstractions.Queries;

public sealed record LocationAggregationDto(
    string Name,
    int Count,
    double? SumNumeric,
    double? AvgNumeric);

using DataStatisticsService.Shared.Constants;

namespace DataStatisticsService.Data.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = ConfigurationSections.RabbitMq;

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "ingested.readings";
}

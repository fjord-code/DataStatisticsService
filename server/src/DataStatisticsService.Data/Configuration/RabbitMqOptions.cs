using DataStatisticsService.Shared.Constants;

namespace DataStatisticsService.Data.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = ConfigurationSections.RabbitMq;

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "weak-app";
    public string QueueName { get; set; } = "ingested-data-processing-queue";
    public string QueueKey { get; set; } = "ingested-data.message";
}

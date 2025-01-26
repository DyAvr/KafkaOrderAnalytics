namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Configuration;

public class KafkaSettings
{
    public required string BootstrapServers { get; init; }
    public required string Topic { get; init; }
    public required string GroupId { get; init; }
    public required int ChannelCapacity { get; init; }
    public required int BufferDelayInSeconds { get; init; }
    public required int MaxRetryAttempts { get; init; }
    public required int RetryDelayInSeconds { get; init; }
}
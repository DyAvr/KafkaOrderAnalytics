using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Configuration;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Services;

public class KafkaBackgroundService : BackgroundService
{
    private readonly KafkaAsyncConsumer<Ignore, OrderEvent> _consumer;
    private readonly ILogger<KafkaBackgroundService> _logger;

    public KafkaBackgroundService(
        IHandler<Ignore, OrderEvent> handler,
        ILogger<KafkaBackgroundService> logger,
        ILogger<KafkaAsyncConsumer<Ignore, OrderEvent>> kafkaAsyncConsumerLogger,
        IDeserializer<OrderEvent>? valueDeserializer,
        IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        var kafkaSettings = kafkaOptions.Value;
        
        _consumer = new KafkaAsyncConsumer<Ignore, OrderEvent>(
            handler,
            kafkaSettings.BootstrapServers,
            kafkaSettings.GroupId,
            kafkaSettings.Topic,
            keyDeserializer: null,
            valueDeserializer,
            kafkaAsyncConsumerLogger,
            kafkaSettings.ChannelCapacity,
            kafkaSettings.BufferDelayInSeconds,
            kafkaSettings.MaxRetryAttempts,
            kafkaSettings.RetryDelayInSeconds);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _consumer.Consume(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
        }
    }
}

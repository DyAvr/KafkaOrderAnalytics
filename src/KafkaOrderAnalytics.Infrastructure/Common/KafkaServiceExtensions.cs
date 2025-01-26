using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

public static class KafkaServiceExtensions
{
    public static IServiceCollection AddKafkaHandler<TKey, TValue, THandler>(
        this IServiceCollection services,
        IDeserializer<TKey>? keyDeserializer = null,
        IDeserializer<TValue>? valueDeserializer = null)
        where THandler : class, IHandler<TKey, TValue>
    {
        services.AddSingleton<IHandler<TKey, TValue>, THandler>();

        if (keyDeserializer != null)
        {
            services.AddSingleton(keyDeserializer);
        }
        if (valueDeserializer != null)
        {
            services.AddSingleton(valueDeserializer);
        }

        return services;
    }
}
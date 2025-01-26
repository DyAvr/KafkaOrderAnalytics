using System;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset Now();
}
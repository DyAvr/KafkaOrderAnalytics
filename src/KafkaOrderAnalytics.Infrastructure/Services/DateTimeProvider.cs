using System;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Interfaces;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now() => DateTimeOffset.UtcNow;
}
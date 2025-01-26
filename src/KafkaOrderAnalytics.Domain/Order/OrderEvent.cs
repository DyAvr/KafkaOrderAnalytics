using System;

using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Order;

public sealed record OrderEvent(
    OrderId OrderId,
    UserId UserId,
    WarehouseId WarehouseId,
    OrderStatus OrderStatus,
    DateTime Moment,
    OrderEventPosition[] Positions);
